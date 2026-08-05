namespace TruthApi.Models.Aws.Networking;

public sealed class AwsNatGatewayAddressInfo
{
    public string AllocationId { get; init; } = "";

    public string AssociationId { get; init; } = "";

    public string NetworkInterfaceId { get; init; } = "";

    public string PrivateIp { get; init; } = "";

    public string PublicIp { get; init; } = "";

    public bool IsPrimary { get; init; }

    public string Status { get; init; } = "";
}
