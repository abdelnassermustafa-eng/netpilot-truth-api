using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using TruthApi.Models.Platform.State;

namespace TruthApi.Services.Platform.State;

/// <summary>
/// Converts provider inventory collections into universal infrastructure
/// resources without requiring the dashboard to understand AWS models.
/// </summary>
public sealed class AwsInventoryNormalizer
{
    private static readonly HashSet<string> IgnoredCollections =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Regions",
            "Warnings"
        };

    public IReadOnlyList<InfrastructureResource> Normalize(
        object inventory,
        string accountId,
        string defaultDomain,
        DateTimeOffset discoveredAt)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        var resources = new List<InfrastructureResource>();

        foreach (var inventoryProperty in inventory
                     .GetType()
                     .GetProperties(BindingFlags.Public |
                                    BindingFlags.Instance))
        {
            if (IgnoredCollections.Contains(inventoryProperty.Name))
            {
                continue;
            }

            var value = inventoryProperty.GetValue(inventory);

            if (value is not IEnumerable items ||
                value is string ||
                value is IDictionary)
            {
                continue;
            }

            var domainId = ResolveDomain(
                defaultDomain,
                inventoryProperty.Name);

            var resourceType = ToResourceType(
                inventoryProperty.Name);

            foreach (var item in items)
            {
                if (item is null ||
                    item is string ||
                    IsScalar(item.GetType()))
                {
                    continue;
                }

                resources.Add(
                    NormalizeItem(
                        item,
                        accountId,
                        domainId,
                        resourceType,
                        discoveredAt));
            }
        }

        return resources;
    }

    private static InfrastructureResource NormalizeItem(
        object item,
        string accountId,
        string domainId,
        string resourceType,
        DateTimeOffset discoveredAt)
    {
        var nativeId = FirstValue(
            item,
            property =>
                property.Name.EndsWith(
                    "Arn",
                    StringComparison.OrdinalIgnoreCase),
            property =>
                property.Name.EndsWith(
                    "Id",
                    StringComparison.OrdinalIgnoreCase));

        if (resourceType.Equals(
                "target-health",
                StringComparison.OrdinalIgnoreCase))
        {
            nativeId = FirstNonEmpty(
                BuildTargetHealthNativeId(item),
                nativeId);
        }

        var displayName = FirstValue(
            item,
            property =>
                property.Name.Equals(
                    "Name",
                    StringComparison.OrdinalIgnoreCase),
            property =>
                property.Name.EndsWith(
                    "Name",
                    StringComparison.OrdinalIgnoreCase));

        if (resourceType.Equals(
                "target-health",
                StringComparison.OrdinalIgnoreCase))
        {
            displayName = FirstNonEmpty(
                BuildTargetHealthDisplayName(item),
                displayName);
        }

        var state = FirstValue(
            item,
            property =>
                property.Name.Equals(
                    "State",
                    StringComparison.OrdinalIgnoreCase),
            property =>
                property.Name.Equals(
                    "Status",
                    StringComparison.OrdinalIgnoreCase),
            property =>
                property.Name.EndsWith(
                    "State",
                    StringComparison.OrdinalIgnoreCase),
            property =>
                property.Name.EndsWith(
                    "Status",
                    StringComparison.OrdinalIgnoreCase));

        var location = GetString(item, "Region");

        var availabilityZone =
            FirstNonEmpty(
                GetString(item, "AvailabilityZone"),
                GetString(item, "ZoneName"));

        var arn = FirstValue(
            item,
            property =>
                property.Name.EndsWith(
                    "Arn",
                    StringComparison.OrdinalIgnoreCase));

        nativeId = FirstNonEmpty(
            nativeId,
            arn,
            displayName,
            Guid.NewGuid().ToString("N"));

        displayName = FirstNonEmpty(
            displayName,
            nativeId);

        return new InfrastructureResource
        {
            ProviderId = "aws",
            AccountId = accountId,
            DomainId = domainId,
            ResourceType = resourceType,
            ResourceId =
                $"aws:{accountId}:{domainId}:{resourceType}:{nativeId}",
            NativeId = nativeId,
            DisplayName = displayName,
            State = state,
            Location = location,
            AvailabilityZone = availabilityZone,
            Arn = arn,
            IconKey = resourceType,
            AccentKey = AccentFor(domainId),
            Properties = ExtractProperties(item),
            Tags = ExtractTags(item),
            DiscoveredAt = discoveredAt
        };
    }

    private static string BuildTargetHealthNativeId(
        object item)
    {
        var targetGroupArn =
            GetString(
                item,
                "TargetGroupArn");

        var targetId =
            GetString(
                item,
                "TargetId");

        if (string.IsNullOrWhiteSpace(targetGroupArn) ||
            string.IsNullOrWhiteSpace(targetId))
        {
            return "";
        }

        var port =
            GetString(
                item,
                "Port");

        return string.Join(
            "|",
            targetGroupArn,
            targetId,
            port);
    }

    private static string BuildTargetHealthDisplayName(
        object item)
    {
        var targetId =
            GetString(
                item,
                "TargetId");

        if (string.IsNullOrWhiteSpace(targetId))
        {
            return "";
        }

        var port =
            GetString(
                item,
                "Port");

        return string.IsNullOrWhiteSpace(port)
            ? targetId
            : $"{targetId}:{port}";
    }

    private static IReadOnlyDictionary<string, string>
        ExtractProperties(object item)
    {
        var properties = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var property in item
                     .GetType()
                     .GetProperties(BindingFlags.Public |
                                    BindingFlags.Instance))
        {
            if (property.Name.Equals(
                    "Tags",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var value = property.GetValue(item);

            if (value is null)
            {
                continue;
            }

            if (IsScalar(value.GetType()))
            {
                properties[property.Name] =
                    Convert.ToString(value) ?? "";
                continue;
            }

            if (value is IEnumerable enumerable &&
                value is not string &&
                value is not IDictionary)
            {
                var values =
                    enumerable
                        .Cast<object?>()
                        .ToList();

                properties[property.Name + "Count"] =
                    values.Count.ToString();

                if (values.Count > 0 &&
                    values.All(item =>
                        item is not null &&
                        IsScalar(item.GetType())))
                {
                    properties[property.Name] =
                        string.Join(
                            ",",
                            values
                                .Select(item =>
                                    Convert.ToString(item))
                                .Where(item =>
                                    !string.IsNullOrWhiteSpace(item)));
                }
            }
        }

        AddInternetGatewayAttachmentProperties(
            item,
            properties);

        AddEbsVolumeAttachmentProperties(
            item,
            properties);

        AddLoadBalancerZoneProperties(
            item,
            properties);

        return properties;
    }

    private static void AddInternetGatewayAttachmentProperties(
        object item,
        IDictionary<string, string> properties)
    {
        var attachmentsProperty = item
            .GetType()
            .GetProperty(
                "Attachments",
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.IgnoreCase);

        if (attachmentsProperty?.GetValue(item) is not
            IEnumerable attachments ||
            attachmentsProperty.PropertyType == typeof(string))
        {
            return;
        }

        var attachedVpcIds = new List<string>();
        var attachmentStates = new List<string>();

        foreach (var attachment in attachments)
        {
            if (attachment is null)
            {
                continue;
            }

            var vpcId = GetString(
                attachment,
                "VpcId");

            if (!string.IsNullOrWhiteSpace(vpcId))
            {
                attachedVpcIds.Add(vpcId);
            }

            var state = GetString(
                attachment,
                "State");

            if (!string.IsNullOrWhiteSpace(state))
            {
                attachmentStates.Add(state);
            }
        }

        if (attachedVpcIds.Count > 0)
        {
            properties["AttachedVpcIds"] =
                string.Join(
                    ",",
                    attachedVpcIds.Distinct(
                        StringComparer.OrdinalIgnoreCase));
        }

        if (attachmentStates.Count > 0)
        {
            properties["AttachmentStates"] =
                string.Join(
                    ",",
                    attachmentStates.Distinct(
                        StringComparer.OrdinalIgnoreCase));
        }
    }

    private static void AddEbsVolumeAttachmentProperties(
        object item,
        IDictionary<string, string> properties)
    {
        var volumeId = GetString(
            item,
            "VolumeId");

        if (string.IsNullOrWhiteSpace(volumeId))
        {
            return;
        }

        var attachmentsProperty = item
            .GetType()
            .GetProperty(
                "Attachments",
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.IgnoreCase);

        if (attachmentsProperty?.GetValue(item) is not
            IEnumerable attachments ||
            attachmentsProperty.PropertyType == typeof(string))
        {
            return;
        }

        var attachedInstanceIds = new List<string>();
        var attachmentDevices = new List<string>();

        foreach (var attachment in attachments)
        {
            if (attachment is null)
            {
                continue;
            }

            var instanceId = GetString(
                attachment,
                "InstanceId");

            if (!string.IsNullOrWhiteSpace(instanceId))
            {
                attachedInstanceIds.Add(instanceId);
            }

            var device = GetString(
                attachment,
                "Device");

            if (!string.IsNullOrWhiteSpace(device))
            {
                attachmentDevices.Add(device);
            }
        }

        if (attachedInstanceIds.Count > 0)
        {
            properties["AttachedInstanceIds"] =
                string.Join(
                    ",",
                    attachedInstanceIds.Distinct(
                        StringComparer.OrdinalIgnoreCase));
        }

        if (attachmentDevices.Count > 0)
        {
            properties["AttachmentDevices"] =
                string.Join(
                    ",",
                    attachmentDevices.Distinct(
                        StringComparer.OrdinalIgnoreCase));
        }
    }

    private static void AddLoadBalancerZoneProperties(
        object item,
        IDictionary<string, string> properties)
    {
        var loadBalancerArn = GetString(
            item,
            "LoadBalancerArn");

        if (string.IsNullOrWhiteSpace(loadBalancerArn))
        {
            return;
        }

        var zonesProperty = item
            .GetType()
            .GetProperty(
                "AvailabilityZones",
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.IgnoreCase);

        if (zonesProperty?.GetValue(item) is not
            IEnumerable zones ||
            zonesProperty.PropertyType == typeof(string))
        {
            return;
        }

        var subnetIds = new List<string>();
        var availabilityZoneNames = new List<string>();

        foreach (var zone in zones)
        {
            if (zone is null)
            {
                continue;
            }

            var subnetId = GetString(
                zone,
                "SubnetId");

            if (!string.IsNullOrWhiteSpace(subnetId))
            {
                subnetIds.Add(subnetId);
            }

            var zoneName = GetString(
                zone,
                "ZoneName");

            if (!string.IsNullOrWhiteSpace(zoneName))
            {
                availabilityZoneNames.Add(zoneName);
            }
        }

        if (subnetIds.Count > 0)
        {
            properties["SubnetIds"] =
                string.Join(
                    ",",
                    subnetIds.Distinct(
                        StringComparer.OrdinalIgnoreCase));
        }

        if (availabilityZoneNames.Count > 0)
        {
            properties["AvailabilityZoneNames"] =
                string.Join(
                    ",",
                    availabilityZoneNames.Distinct(
                        StringComparer.OrdinalIgnoreCase));
        }
    }

    private static IReadOnlyDictionary<string, string>
        ExtractTags(object item)
    {
        var tagsProperty = item
            .GetType()
            .GetProperty(
                "Tags",
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.IgnoreCase);

        if (tagsProperty?.GetValue(item) is
            IReadOnlyDictionary<string, string> readOnlyTags)
        {
            return new Dictionary<string, string>(
                readOnlyTags,
                StringComparer.OrdinalIgnoreCase);
        }

        if (tagsProperty?.GetValue(item) is
            IDictionary<string, string> tags)
        {
            return new Dictionary<string, string>(
                tags,
                StringComparer.OrdinalIgnoreCase);
        }

        return new Dictionary<string, string>();
    }

    private static string FirstValue(
        object item,
        params Func<PropertyInfo, bool>[] selectors)
    {
        var properties = item
            .GetType()
            .GetProperties(BindingFlags.Public |
                           BindingFlags.Instance);

        foreach (var selector in selectors)
        {
            var match = properties.FirstOrDefault(selector);

            if (match is null)
            {
                continue;
            }

            var value = Convert.ToString(
                match.GetValue(item));

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return "";
    }

    private static string GetString(
        object item,
        string propertyName)
    {
        var property = item
            .GetType()
            .GetProperty(
                propertyName,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.IgnoreCase);

        return Convert.ToString(
                   property?.GetValue(item))
               ?? "";
    }

    private static string ResolveDomain(
        string defaultDomain,
        string collectionName)
    {
        if (!defaultDomain.Equals(
                "compute",
                StringComparison.OrdinalIgnoreCase))
        {
            return defaultDomain;
        }

        var autoScalingTerms = new[]
        {
            "AutoScaling",
            "LaunchConfiguration",
            "ScalingPolicy",
            "ScheduledAction",
            "ScalingActivity",
            "LifecycleHook",
            "WarmPool",
            "InstanceRefresh"
        };

        return autoScalingTerms.Any(
            term => collectionName.Contains(
                term,
                StringComparison.OrdinalIgnoreCase))
            ? "auto-scaling"
            : "compute";
    }

    private static string ToResourceType(
        string collectionName)
    {
        var singular = collectionName switch
        {
            "Vpcs" => "Vpc",
            "Images" => "Ami",
            "KeyPairs" => "KeyPair",
            "TargetHealth" => "TargetHealth",
            _ when collectionName.EndsWith(
                "ies",
                StringComparison.OrdinalIgnoreCase) =>
                collectionName[..^3] + "y",
            _ when collectionName.EndsWith(
                "s",
                StringComparison.OrdinalIgnoreCase) =>
                collectionName[..^1],
            _ => collectionName
        };

        return Regex.Replace(
                singular,
                "([a-z0-9])([A-Z])",
                "$1-$2")
            .ToLowerInvariant();
    }

    private static bool IsScalar(Type type)
    {
        var actual = Nullable.GetUnderlyingType(type) ?? type;

        return actual.IsPrimitive ||
               actual.IsEnum ||
               actual == typeof(string) ||
               actual == typeof(decimal) ||
               actual == typeof(DateTime) ||
               actual == typeof(DateTimeOffset) ||
               actual == typeof(Guid);
    }

    private static string AccentFor(string domainId)
    {
        return domainId switch
        {
            "identity" => "amber",
            "networking" => "purple",
            "compute" => "blue",
            "auto-scaling" => "green",
            "load-balancing" => "cyan",
            "storage" => "teal",
            "database" => "emerald",
            "containers" => "indigo",
            "security" => "red",
            "observability" => "yellow",
            "ai" => "violet",
            _ => "default"
        };
    }

    private static string FirstNonEmpty(
        params string?[] values)
    {
        return values.FirstOrDefault(
                   value => !string.IsNullOrWhiteSpace(value))
               ?? "";
    }
}
