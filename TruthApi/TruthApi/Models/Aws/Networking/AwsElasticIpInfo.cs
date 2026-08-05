namespace TruthApi.Models.Aws.Networking;

public sealed class AwsElasticIpInfo
{
    public string AllocationId { get; init; } = "";

    public string AssociationId { get; init; } = "";

    public string PublicIp { get; init; } = "";

    public string PrivateIpAddress { get; init; } = "";

    public string NetworkInterfaceId { get; init; } = "";

    public string NetworkInterfaceOwnerId { get; init; } = "";

    public string InstanceId { get; init; } = "";

    public string Domain { get; init; } = "";

    public string NetworkBorderGroup { get; init; } = "";

    public string PublicIpv4Pool { get; init; } = "";

    public string CustomerOwnedIp { get; init; } = "";

    public string CustomerOwnedIpv4Pool { get; init; } = "";

    public string Name { get; init; } = "";

    public string Region { get; init; } = "";

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
