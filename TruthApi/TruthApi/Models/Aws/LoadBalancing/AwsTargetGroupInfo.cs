namespace TruthApi.Models.Aws.LoadBalancing;

/// <summary>
/// Represents an ELBv2 target group, its health-check configuration,
/// associated load balancers, attributes, and tags.
/// </summary>
public sealed class AwsTargetGroupInfo
{
    public string TargetGroupArn { get; init; } = "";

    public string Name { get; init; } = "";

    public string Protocol { get; init; } = "";

    public string ProtocolVersion { get; init; } = "";

    public int? Port { get; init; }

    public string TargetType { get; init; } = "";

    public string IpAddressType { get; init; } = "";

    public string VpcId { get; init; } = "";

    public bool HealthCheckEnabled { get; init; }

    public string HealthCheckProtocol { get; init; } = "";

    public int? HealthCheckPort { get; init; }

    public string HealthCheckPortExpression { get; init; } = "";

    public string HealthCheckPath { get; init; } = "";

    public int? HealthCheckIntervalSeconds { get; init; }

    public int? HealthCheckTimeoutSeconds { get; init; }

    public int? HealthyThresholdCount { get; init; }

    public int? UnhealthyThresholdCount { get; init; }

    public string MatcherHttpCode { get; init; } = "";

    public string MatcherGrpcCode { get; init; } = "";

    public string Region { get; init; } = "";

    public IReadOnlyList<string> LoadBalancerArns { get; init; } =
        Array.Empty<string>();

    public IReadOnlyDictionary<string, string> Attributes { get; init; } =
        new Dictionary<string, string>();

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
