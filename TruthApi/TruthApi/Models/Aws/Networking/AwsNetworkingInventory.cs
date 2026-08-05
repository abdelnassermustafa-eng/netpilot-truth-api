namespace TruthApi.Models.Aws.Networking;

/// <summary>
/// Represents live AWS networking inventory for one account and one or more
/// selected Regions.
///
/// Additional resource collections will be added without changing the
/// purpose of this model.
/// </summary>
public sealed class AwsNetworkingInventory
{
    public string AccountId { get; init; } = "";

    public IReadOnlyList<string> Regions { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<AwsVpcInfo> Vpcs { get; init; } =
        Array.Empty<AwsVpcInfo>();

    public IReadOnlyList<AwsSubnetInfo> Subnets { get; init; } =
        Array.Empty<AwsSubnetInfo>();

    public IReadOnlyList<AwsRouteTableInfo> RouteTables { get; init; } =
        Array.Empty<AwsRouteTableInfo>();

    public IReadOnlyList<AwsInternetGatewayInfo> InternetGateways
    { get; init; } =
        Array.Empty<AwsInternetGatewayInfo>();

    public IReadOnlyList<AwsNatGatewayInfo> NatGateways { get; init; } =
        Array.Empty<AwsNatGatewayInfo>();

    public IReadOnlyList<AwsSecurityGroupInfo> SecurityGroups
    { get; init; } =
        Array.Empty<AwsSecurityGroupInfo>();

    public IReadOnlyList<AwsNetworkAclInfo> NetworkAcls { get; init; } =
        Array.Empty<AwsNetworkAclInfo>();

    public IReadOnlyList<AwsVpcEndpointInfo> VpcEndpoints { get; init; } =
        Array.Empty<AwsVpcEndpointInfo>();

    public IReadOnlyList<AwsElasticIpInfo> ElasticIps { get; init; } =
        Array.Empty<AwsElasticIpInfo>();

    public IReadOnlyList<AwsNetworkInterfaceInfo> NetworkInterfaces
    { get; init; } =
        Array.Empty<AwsNetworkInterfaceInfo>();

    public DateTimeOffset DiscoveredAt { get; init; } =
        DateTimeOffset.UtcNow;

    public IReadOnlyList<string> Warnings { get; init; } =
        Array.Empty<string>();
}
