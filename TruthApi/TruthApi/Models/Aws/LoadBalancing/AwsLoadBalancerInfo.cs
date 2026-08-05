namespace TruthApi.Models.Aws.LoadBalancing;

/// <summary>
/// Represents an ELBv2 Application, Network, or Gateway Load Balancer.
/// </summary>
public sealed class AwsLoadBalancerInfo
{
    public string LoadBalancerArn { get; init; } = "";

    public string Name { get; init; } = "";

    public string Type { get; init; } = "";

    public string Scheme { get; init; } = "";

    public string State { get; init; } = "";

    public string StateReason { get; init; } = "";

    public string IpAddressType { get; init; } = "";

    public string VpcId { get; init; } = "";

    public string DnsName { get; init; } = "";

    public string CanonicalHostedZoneId { get; init; } = "";

    public string CustomerOwnedIpv4Pool { get; init; } = "";

    public bool EnforceSecurityGroupInboundRulesOnPrivateLinkTraffic
    { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string Region { get; init; } = "";

    public IReadOnlyList<string> SecurityGroupIds { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<AwsLoadBalancerZoneInfo> AvailabilityZones
    { get; init; } =
        Array.Empty<AwsLoadBalancerZoneInfo>();

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
