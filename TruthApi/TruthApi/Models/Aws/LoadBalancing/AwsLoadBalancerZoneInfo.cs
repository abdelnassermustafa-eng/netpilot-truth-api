namespace TruthApi.Models.Aws.LoadBalancing;

public sealed class AwsLoadBalancerZoneInfo
{
    public string ZoneName { get; init; } = "";

    public string SubnetId { get; init; } = "";

    public string OutpostId { get; init; } = "";

    public IReadOnlyList<string> IpAddresses { get; init; } =
        Array.Empty<string>();
}
