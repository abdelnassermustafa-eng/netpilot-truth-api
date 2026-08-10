using System;
using System.Collections.Generic;
using System.Linq;
using TruthApi.Models.Platform.State;

namespace TruthApi.Services.Platform.State;

/// <summary>
/// Converts normalized AWS resource references into universal
/// infrastructure relationships.
///
/// Provider knowledge stops here. Consumers receive only universal
/// InfrastructureRelationship objects.
/// </summary>
public sealed class AwsRelationshipBuilder
{
    public IReadOnlyList<InfrastructureRelationship> Build(
        IReadOnlyList<InfrastructureResource> resources)
    {
        ArgumentNullException.ThrowIfNull(resources);

        var byNativeId =
            resources
                .Where(resource =>
                    !string.IsNullOrWhiteSpace(
                        resource.NativeId))
                .GroupBy(
                    resource => resource.NativeId,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList(),
                    StringComparer.OrdinalIgnoreCase);

        var relationships =
            new List<InfrastructureRelationship>();

        var identities =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var resource in resources)
        {
            // ------------------------------------------------
            // Universal AWS containment
            // ------------------------------------------------

            AddReference(
                resource,
                "VpcId",
                "vpc",
                "member-of",
                "Resource belongs to VPC",
                byNativeId,
                relationships,
                identities);

            AddReference(
                resource,
                "SubnetId",
                "subnet",
                "hosted-on",
                "Resource is hosted in subnet",
                byNativeId,
                relationships,
                identities);

            // ------------------------------------------------
            // EC2 instance connectivity
            // ------------------------------------------------

            if (IsType(resource, "instance"))
            {
                AddReferences(
                    resource,
                    "NetworkInterfaceIds",
                    "network-interface",
                    "attached-to",
                    "Instance uses network interface",
                    byNativeId,
                    relationships,
                    identities);
            }

            // ------------------------------------------------
            // Elastic IP attachment
            // ------------------------------------------------

            if (IsType(resource, "elastic-ip"))
            {
                AddReference(
                    resource,
                    "InstanceId",
                    "instance",
                    "attached-to",
                    "Elastic IP is associated with instance",
                    byNativeId,
                    relationships,
                    identities);

                AddReference(
                    resource,
                    "NetworkInterfaceId",
                    "network-interface",
                    "attached-to",
                    "Elastic IP is associated with network interface",
                    byNativeId,
                    relationships,
                    identities);
            }

            // ------------------------------------------------
            // EC2 instance security
            // ------------------------------------------------

            if (IsType(resource, "instance"))
            {
                AddReferences(
                    resource,
                    "SecurityGroupIds",
                    "security-group",
                    "secured-by",
                    "EC2 instance is protected by security group",
                    byNativeId,
                    relationships,
                    identities);
            }

            // ------------------------------------------------
            // EBS volume attachment
            // ------------------------------------------------

            if (IsType(resource, "volume"))
            {
                AddReferences(
                    resource,
                    "AttachedInstanceIds",
                    "instance",
                    "attached-to",
                    "EBS volume is attached to EC2 instance",
                    byNativeId,
                    relationships,
                    identities);
            }

            // ------------------------------------------------
            // Network interface security
            // ------------------------------------------------

            if (IsType(resource, "network-interface"))
            {
                AddReferences(
                    resource,
                    "SecurityGroupIds",
                    "security-group",
                    "secured-by",
                    "Network interface is protected by security group",
                    byNativeId,
                    relationships,
                    identities);
            }

            // ------------------------------------------------
            // Internet gateway attachment
            // ------------------------------------------------

            if (IsType(resource, "internet-gateway"))
            {
                AddReferences(
                    resource,
                    "AttachedVpcIds",
                    "vpc",
                    "attached-to",
                    "Internet gateway is attached to VPC",
                    byNativeId,
                    relationships,
                    identities);
            }

            // ------------------------------------------------
            // VPC endpoints
            // ------------------------------------------------

            if (IsType(resource, "vpc-endpoint"))
            {
                AddReferences(
                    resource,
                    "RouteTableIds",
                    "route-table",
                    "uses",
                    "VPC endpoint uses route table",
                    byNativeId,
                    relationships,
                    identities);

                AddReferences(
                    resource,
                    "SubnetIds",
                    "subnet",
                    "hosted-on",
                    "VPC endpoint is hosted in subnet",
                    byNativeId,
                    relationships,
                    identities);

                AddReferences(
                    resource,
                    "NetworkInterfaceIds",
                    "network-interface",
                    "attached-to",
                    "VPC endpoint uses network interface",
                    byNativeId,
                    relationships,
                    identities);
            }

            // ------------------------------------------------
            // Load balancing
            // ------------------------------------------------

            if (IsType(resource, "load-balancer"))
            {
                AddReferences(
                    resource,
                    "SecurityGroupIds",
                    "security-group",
                    "secured-by",
                    "Load balancer is protected by security group",
                    byNativeId,
                    relationships,
                    identities);

                AddReferences(
                    resource,
                    "SubnetIds",
                    "subnet",
                    "hosted-on",
                    "Load balancer is hosted in subnet",
                    byNativeId,
                    relationships,
                    identities);
            }

            if (IsType(resource, "target-group"))
            {
                AddReferences(
                    resource,
                    "LoadBalancerArns",
                    "load-balancer",
                    "attached-to",
                    "Target group is associated with load balancer",
                    byNativeId,
                    relationships,
                    identities);
            }

            // ------------------------------------------------
            // Auto Scaling
            // ------------------------------------------------

            if (IsType(resource, "auto-scaling-group"))
            {
                AddReferences(
                    resource,
                    "SubnetIds",
                    "subnet",
                    "uses",
                    "Auto Scaling group uses subnet",
                    byNativeId,
                    relationships,
                    identities);
            }

            if (IsType(resource, "launch-configuration"))
            {
                AddReferences(
                    resource,
                    "SecurityGroupIds",
                    "security-group",
                    "secured-by",
                    "Launch configuration uses security group",
                    byNativeId,
                    relationships,
                    identities);
            }
        }

        AddTargetGroupTargets(
            resources,
            byNativeId,
            relationships,
            identities);

        AddRouteTableAssociations(
            resources,
            byNativeId,
            relationships,
            identities);

        AddRouteTableTargets(
            resources,
            byNativeId,
            relationships,
            identities);

        return relationships;
    }

    private static void AddTargetGroupTargets(
        IReadOnlyList<InfrastructureResource> resources,
        IReadOnlyDictionary<
            string,
            List<InfrastructureResource>> byNativeId,
        ICollection<InfrastructureRelationship> output,
        ISet<string> identities)
    {
        foreach (var targetHealth in
                 resources.Where(resource =>
                     IsType(resource, "target-health")))
        {
            var targetGroupArn =
                GetProperty(
                    targetHealth,
                    "TargetGroupArn");

            var targetId =
                GetProperty(
                    targetHealth,
                    "TargetId");

            if (string.IsNullOrWhiteSpace(targetGroupArn) ||
                string.IsNullOrWhiteSpace(targetId) ||
                !byNativeId.TryGetValue(
                    targetGroupArn,
                    out var targetGroupCandidates))
            {
                continue;
            }

            var targetGroup =
                targetGroupCandidates.FirstOrDefault(candidate =>
                    IsType(candidate, "target-group"));

            if (targetGroup is null ||
                !GetProperty(
                        targetGroup,
                        "TargetType")
                    .Equals(
                        "instance",
                        StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            AddResolvedReference(
                targetGroup,
                targetId,
                "instance",
                "targets",
                "Target group routes traffic to registered EC2 instance",
                byNativeId,
                output,
                identities);

            AddResolvedReference(
                targetHealth,
                targetGroupArn,
                "target-group",
                "member-of",
                "Target health belongs to target group",
                byNativeId,
                output,
                identities);

            AddResolvedReference(
                targetHealth,
                targetId,
                "instance",
                "associated-with",
                "Target health describes registered EC2 instance",
                byNativeId,
                output,
                identities);
        }
    }

    private static void AddRouteTableAssociations(
        IReadOnlyList<InfrastructureResource> resources,
        IReadOnlyDictionary<
            string,
            List<InfrastructureResource>> byNativeId,
        ICollection<InfrastructureRelationship> output,
        ISet<string> identities)
    {
        var routeTables =
            resources
                .Where(resource =>
                    IsType(resource, "route-table"))
                .ToList();

        var subnets =
            resources
                .Where(resource =>
                    IsType(resource, "subnet"))
                .ToList();

        var explicitlyAssociatedSubnetIds =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var routeTable in routeTables)
        {
            if (!routeTable.Properties.TryGetValue(
                    "AssociatedSubnetIds",
                    out var serializedIds) ||
                string.IsNullOrWhiteSpace(serializedIds))
            {
                continue;
            }

            foreach (var subnetId in
                     SplitIds(serializedIds))
            {
                explicitlyAssociatedSubnetIds.Add(
                    subnetId);

                AddResolvedReference(
                    routeTable,
                    subnetId,
                    "subnet",
                    "associated-with",
                    "Route table is explicitly associated with subnet",
                    byNativeId,
                    output,
                    identities);
            }
        }

        var mainRouteTablesByVpc =
            routeTables
                .Where(routeTable =>
                    GetBooleanProperty(
                        routeTable,
                        "IsMain"))
                .Select(routeTable =>
                    new
                    {
                        RouteTable = routeTable,

                        VpcId =
                            GetProperty(
                                routeTable,
                                "VpcId")
                    })
                .Where(item =>
                    !string.IsNullOrWhiteSpace(
                        item.VpcId))
                .GroupBy(
                    item => item.VpcId,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(item =>
                            item.RouteTable)
                        .First(),
                    StringComparer.OrdinalIgnoreCase);

        foreach (var subnet in subnets)
        {
            if (explicitlyAssociatedSubnetIds.Contains(
                    subnet.NativeId))
            {
                continue;
            }

            var vpcId =
                GetProperty(
                    subnet,
                    "VpcId");

            if (string.IsNullOrWhiteSpace(vpcId) ||
                !mainRouteTablesByVpc.TryGetValue(
                    vpcId,
                    out var mainRouteTable))
            {
                continue;
            }

            AddResolvedReference(
                mainRouteTable,
                subnet.NativeId,
                "subnet",
                "associated-with",
                "Subnet inherits the VPC main route table",
                byNativeId,
                output,
                identities);
        }
    }

    private static void AddRouteTableTargets(
        IReadOnlyList<InfrastructureResource> resources,
        IReadOnlyDictionary<
            string,
            List<InfrastructureResource>> byNativeId,
        ICollection<InfrastructureRelationship> output,
        ISet<string> identities)
    {
        foreach (var routeTable in
                 resources.Where(resource =>
                     IsType(resource, "route-table")))
        {
            var serializedTargets =
                GetProperty(
                    routeTable,
                    "RouteTargets");

            if (string.IsNullOrWhiteSpace(
                    serializedTargets))
            {
                continue;
            }

            foreach (var encodedTarget in
                     SplitIds(serializedTargets))
            {
                var separator =
                    encodedTarget.IndexOf('|');

                if (separator <= 0 ||
                    separator >=
                    encodedTarget.Length - 1)
                {
                    continue;
                }

                var targetType =
                    encodedTarget[..separator];

                var targetNativeId =
                    encodedTarget[(separator + 1)..];

                var targetResourceType =
                    ResolveRouteTargetResourceType(
                        targetType);

                if (string.IsNullOrWhiteSpace(
                        targetResourceType))
                {
                    continue;
                }

                AddResolvedReference(
                    routeTable,
                    targetNativeId,
                    targetResourceType,
                    "routes-to",
                    $"Route table routes to {targetType}",
                    byNativeId,
                    output,
                    identities);
            }
        }
    }

    private static string ResolveRouteTargetResourceType(
        string targetType)
    {
        return targetType.ToLowerInvariant() switch
        {
            "gateway" =>
                "internet-gateway",

            "natgateway" =>
                "nat-gateway",

            "instance" =>
                "instance",

            "networkinterface" =>
                "network-interface",

            "transitgateway" =>
                "transit-gateway",

            "vpcpeeringconnection" =>
                "vpc-peering-connection",

            "egressonlyinternetgateway" =>
                "egress-only-internet-gateway",

            _ => ""
        };
    }

    private static IEnumerable<string> SplitIds(
        string serializedIds)
    {
        return serializedIds
            .Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Where(value =>
                !string.IsNullOrWhiteSpace(value))
            .Distinct(
                StringComparer.OrdinalIgnoreCase);
    }

    private static string GetProperty(
        InfrastructureResource resource,
        string propertyName)
    {
        return resource.Properties.TryGetValue(
                propertyName,
                out var value)
            ? value
            : "";
    }

    private static bool GetBooleanProperty(
        InfrastructureResource resource,
        string propertyName)
    {
        return bool.TryParse(
                   GetProperty(
                       resource,
                       propertyName),
                   out var value) &&
               value;
    }

    private static bool IsType(
        InfrastructureResource resource,
        string resourceType)
    {
        return resource.ResourceType.Equals(
            resourceType,
            StringComparison.OrdinalIgnoreCase);
    }

    private static void AddReference(
        InfrastructureResource source,
        string propertyName,
        string targetResourceType,
        string relationshipType,
        string description,
        IReadOnlyDictionary<
            string,
            List<InfrastructureResource>> byNativeId,
        ICollection<InfrastructureRelationship> output,
        ISet<string> identities)
    {
        if (!source.Properties.TryGetValue(
                propertyName,
                out var targetNativeId) ||
            string.IsNullOrWhiteSpace(targetNativeId))
        {
            return;
        }

        AddResolvedReference(
            source,
            targetNativeId,
            targetResourceType,
            relationshipType,
            description,
            byNativeId,
            output,
            identities);
    }

    private static void AddReferences(
        InfrastructureResource source,
        string propertyName,
        string targetResourceType,
        string relationshipType,
        string description,
        IReadOnlyDictionary<
            string,
            List<InfrastructureResource>> byNativeId,
        ICollection<InfrastructureRelationship> output,
        ISet<string> identities)
    {
        if (!source.Properties.TryGetValue(
                propertyName,
                out var serializedIds) ||
            string.IsNullOrWhiteSpace(serializedIds))
        {
            return;
        }

        foreach (var targetNativeId in
                 SplitIds(serializedIds))
        {
            AddResolvedReference(
                source,
                targetNativeId,
                targetResourceType,
                relationshipType,
                description,
                byNativeId,
                output,
                identities);
        }
    }

    private static void AddResolvedReference(
        InfrastructureResource source,
        string targetNativeId,
        string targetResourceType,
        string relationshipType,
        string description,
        IReadOnlyDictionary<
            string,
            List<InfrastructureResource>> byNativeId,
        ICollection<InfrastructureRelationship> output,
        ISet<string> identities)
    {
        if (!byNativeId.TryGetValue(
                targetNativeId,
                out var candidates))
        {
            return;
        }

        var target =
            candidates.FirstOrDefault(candidate =>
                candidate.ResourceType.Equals(
                    targetResourceType,
                    StringComparison.OrdinalIgnoreCase));

        if (target is null)
        {
            return;
        }

        if (source.ResourceId.Equals(
                target.ResourceId,
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var identity =
            $"{source.ResourceId}\u001F" +
            $"{relationshipType}\u001F" +
            $"{target.ResourceId}";

        if (!identities.Add(identity))
        {
            return;
        }

        output.Add(
            new InfrastructureRelationship
            {
                Type = relationshipType,

                SourceResourceId =
                    source.ResourceId,

                TargetResourceId =
                    target.ResourceId,

                Description =
                    description
            });
    }
}
