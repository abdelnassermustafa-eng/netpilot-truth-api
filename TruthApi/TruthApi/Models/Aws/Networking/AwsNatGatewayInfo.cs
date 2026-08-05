namespace TruthApi.Models.Aws.Networking;

public sealed class AwsNatGatewayInfo
{
    public string NatGatewayId { get; init; } = "";

    public string Name { get; init; } = "";

    public string VpcId { get; init; } = "";

    public string SubnetId { get; init; } = "";

    public string State { get; init; } = "";

    public string ConnectivityType { get; init; } = "";

    public string FailureCode { get; init; } = "";

    public string FailureMessage { get; init; } = "";

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }

    public string Region { get; init; } = "";

    public IReadOnlyList<AwsNatGatewayAddressInfo> Addresses { get; init; } =
        Array.Empty<AwsNatGatewayAddressInfo>();

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
