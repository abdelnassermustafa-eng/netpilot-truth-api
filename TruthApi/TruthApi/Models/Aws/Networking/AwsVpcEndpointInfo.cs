namespace TruthApi.Models.Aws.Networking;

public sealed class AwsVpcEndpointInfo
{
    public string VpcEndpointId { get; init; } = "";

    public string Name { get; init; } = "";

    public string VpcId { get; init; } = "";

    public string ServiceName { get; init; } = "";

    public string EndpointType { get; init; } = "";

    public string State { get; init; } = "";

    public string OwnerId { get; init; } = "";

    public bool PrivateDnsEnabled { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string PolicyDocument { get; init; } = "";

    public string Region { get; init; } = "";

    public IReadOnlyList<string> RouteTableIds { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> SubnetIds { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> NetworkInterfaceIds { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<AwsVpcEndpointSecurityGroupInfo> SecurityGroups
    { get; init; } =
        Array.Empty<AwsVpcEndpointSecurityGroupInfo>();

    public IReadOnlyList<AwsVpcEndpointDnsEntryInfo> DnsEntries
    { get; init; } =
        Array.Empty<AwsVpcEndpointDnsEntryInfo>();

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
