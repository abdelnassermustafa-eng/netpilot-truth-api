using TruthApi.Models.Aws.Networking;
using TruthApi.Models.Platform.State;

namespace TruthApi.Services.Platform.State.Adapters;

public sealed class AwsNetworkingResourceAdapter :
    IInfrastructureResourceAdapter<AwsNetworkingInventory>
{
    public IReadOnlyList<InfrastructureResource> Adapt(
        AwsNetworkingInventory inventory,
        DateTimeOffset discoveredAt)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        var resources = new List<InfrastructureResource>();

        resources.AddRange(inventory.Vpcs.Select(item =>
            Create(
                inventory.AccountId,
                "vpc",
                item.VpcId,
                item.Name,
                item.State,
                item.Region,
                item.Tags,
                discoveredAt,
                new Dictionary<string, string>
                {
                    ["cidrBlock"] = item.CidrBlock,
                    ["isDefault"] = item.IsDefault.ToString()
                })));

        resources.AddRange(inventory.Subnets.Select(item =>
            Create(
                inventory.AccountId,
                "subnet",
                item.SubnetId,
                item.Name,
                item.State,
                item.Region,
                item.Tags,
                discoveredAt,
                new Dictionary<string, string>
                {
                    ["vpcId"] = item.VpcId,
                    ["cidrBlock"] = item.CidrBlock,
                    ["availabilityZone"] =
                        item.AvailabilityZone,
                    ["availabilityZoneId"] =
                        item.AvailabilityZoneId,
                    ["mapPublicIpOnLaunch"] =
                        item.MapPublicIpOnLaunch.ToString(),
                    ["availableIpAddressCount"] =
                        item.AvailableIpAddressCount.ToString()
                },
                availabilityZone: item.AvailabilityZone)));

        resources.AddRange(inventory.RouteTables.Select(item =>
            Create(
                inventory.AccountId,
                "route-table",
                item.RouteTableId,
                item.Name,
                "available",
                item.Region,
                item.Tags,
                discoveredAt,
                new Dictionary<string, string>
                {
                    ["vpcId"] = item.VpcId,
                    ["isMain"] = item.IsMain.ToString(),
                    ["routeCount"] =
                        item.Routes.Count.ToString(),
                    ["associationCount"] =
                        item.Associations.Count.ToString()
                })));

        resources.AddRange(inventory.InternetGateways.Select(item =>
            Create(
                inventory.AccountId,
                "internet-gateway",
                item.InternetGatewayId,
                item.Name,
                item.Attachments.FirstOrDefault()?.State ?? "",
                item.Region,
                item.Tags,
                discoveredAt,
                new Dictionary<string, string>
                {
                    ["attachedVpcIds"] = string.Join(
                        ",",
                        item.Attachments.Select(
                            attachment => attachment.VpcId))
                })));

        resources.AddRange(inventory.NatGateways.Select(item =>
            Create(
                inventory.AccountId,
                "nat-gateway",
                item.NatGatewayId,
                item.Name,
                item.State,
                item.Region,
                item.Tags,
                discoveredAt,
                new Dictionary<string, string>
                {
                    ["vpcId"] = item.VpcId,
                    ["subnetId"] = item.SubnetId
                })));

        resources.AddRange(inventory.VpcEndpoints.Select(item =>
            Create(
                inventory.AccountId,
                "vpc-endpoint",
                item.VpcEndpointId,
                item.Name,
                item.State,
                item.Region,
                EmptyTags(),
                discoveredAt,
                new Dictionary<string, string>
                {
                    ["vpcId"] = item.VpcId,
                    ["serviceName"] = item.ServiceName,
                    ["ownerId"] = item.OwnerId
                })));

        resources.AddRange(inventory.ElasticIps.Select(item =>
            Create(
                inventory.AccountId,
                "elastic-ip",
                item.AllocationId,
                item.Name,
                string.IsNullOrWhiteSpace(item.AssociationId)
                    ? "unassociated"
                    : "associated",
                item.Region,
                EmptyTags(),
                discoveredAt,
                new Dictionary<string, string>
                {
                    ["associationId"] = item.AssociationId,
                    ["instanceId"] = item.InstanceId,
                    ["networkInterfaceId"] =
                        item.NetworkInterfaceId
                })));

        resources.AddRange(inventory.NetworkInterfaces.Select(item =>
            Create(
                inventory.AccountId,
                "network-interface",
                item.NetworkInterfaceId,
                item.Name,
                item.Status,
                item.Region,
                EmptyTags(),
                discoveredAt,
                new Dictionary<string, string>
                {
                    ["vpcId"] = item.VpcId,
                    ["subnetId"] = item.SubnetId,
                    ["ownerId"] = item.OwnerId,
                    ["privateDnsName"] =
                        item.PrivateDnsName
                })));

        resources.AddRange(inventory.SecurityGroups.Select(item =>
            Create(
                inventory.AccountId,
                "security-group",
                item.GroupId,
                FirstNonEmpty(item.Name, item.GroupName),
                "available",
                item.Region,
                item.Tags,
                discoveredAt,
                new Dictionary<string, string>
                {
                    ["groupName"] = item.GroupName,
                    ["description"] = item.Description,
                    ["vpcId"] = item.VpcId,
                    ["ownerId"] = item.OwnerId,
                    ["ingressRuleCount"] =
                        item.IngressRules.Count.ToString(),
                    ["egressRuleCount"] =
                        item.EgressRules.Count.ToString()
                })));

        resources.AddRange(inventory.NetworkAcls.Select(item =>
            Create(
                inventory.AccountId,
                "network-acl",
                item.NetworkAclId,
                item.Name,
                "available",
                item.Region,
                item.Tags,
                discoveredAt,
                new Dictionary<string, string>
                {
                    ["vpcId"] = item.VpcId,
                    ["ownerId"] = item.OwnerId,
                    ["isDefault"] = item.IsDefault.ToString(),
                    ["associationCount"] =
                        item.Associations.Count.ToString(),
                    ["entryCount"] =
                        item.Entries.Count.ToString()
                })));

        return resources;
    }

    private static InfrastructureResource Create(
        string accountId,
        string resourceType,
        string nativeId,
        string? displayName,
        string? state,
        string? location,
        IReadOnlyDictionary<string, string>? tags,
        DateTimeOffset discoveredAt,
        IReadOnlyDictionary<string, string>? properties = null,
        string availabilityZone = "")
    {
        return new InfrastructureResource
        {
            ProviderId = "aws",
            AccountId = accountId,
            DomainId = "networking",
            ResourceType = resourceType,
            NativeId = nativeId,
            ResourceId =
                $"aws:{accountId}:networking:{resourceType}:{nativeId}",
            DisplayName = FirstNonEmpty(displayName, nativeId),
            State = state ?? "",
            Location = location ?? "",
            AvailabilityZone = availabilityZone,
            IconKey = resourceType,
            AccentKey = "purple",
            Properties = properties ??
                new Dictionary<string, string>(),
            Tags = tags ?? EmptyTags(),
            DiscoveredAt = discoveredAt
        };
    }

    private static string FirstNonEmpty(
        params string?[] values)
    {
        return values.FirstOrDefault(
                   value => !string.IsNullOrWhiteSpace(value))
               ?? "";
    }

    private static IReadOnlyDictionary<string, string> EmptyTags()
    {
        return new Dictionary<string, string>();
    }
}
