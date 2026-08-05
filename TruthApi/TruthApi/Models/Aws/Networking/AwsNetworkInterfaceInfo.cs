namespace TruthApi.Models.Aws.Networking;

public sealed class AwsNetworkInterfaceInfo
{
    public string NetworkInterfaceId { get; init; } = "";

    public string Name { get; init; } = "";

    public string Description { get; init; } = "";

    public string InterfaceType { get; init; } = "";

    public string Status { get; init; } = "";

    public string VpcId { get; init; } = "";

    public string SubnetId { get; init; } = "";

    public string AvailabilityZone { get; init; } = "";

    public string OwnerId { get; init; } = "";

    public string RequesterId { get; init; } = "";

    public bool RequesterManaged { get; init; }

    public bool SourceDestinationCheck { get; init; }

    public string MacAddress { get; init; } = "";

    public string PrivateIpAddress { get; init; } = "";

    public string PrivateDnsName { get; init; } = "";

    public string Region { get; init; } = "";

    public IReadOnlyList<string> SecurityGroupIds { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> Ipv6Addresses { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<AwsNetworkInterfacePrivateIpInfo> PrivateIpAddresses
    { get; init; } =
        Array.Empty<AwsNetworkInterfacePrivateIpInfo>();

    public AwsNetworkInterfaceAttachmentInfo? Attachment { get; init; }

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
