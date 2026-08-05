namespace TruthApi.Models.Aws.LoadBalancing;

/// <summary>
/// Represents one operational attribute configured on an ELBv2
/// load balancer.
/// </summary>
public sealed class AwsLoadBalancerAttributeInfo
{
    public string LoadBalancerArn { get; init; } = "";

    public string Key { get; init; } = "";

    public string Value { get; init; } = "";

    public string Region { get; init; } = "";
}
