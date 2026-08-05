namespace TruthApi.Models.Aws.Networking;

public sealed class AwsNetworkInterfacePrivateIpInfo
{
    public string PrivateIpAddress { get; init; } = "";

    public bool IsPrimary { get; init; }

    public string PrivateDnsName { get; init; } = "";

    public string PublicIp { get; init; } = "";

    public string PublicDnsName { get; init; } = "";

    public string AllocationId { get; init; } = "";

    public string AssociationId { get; init; } = "";
}
