namespace TruthApi.Models.Aws.LoadBalancing;

/// <summary>
/// Complete Elastic Load Balancing inventory.
/// </summary>
public sealed class AwsLoadBalancingInventory
{
    public IReadOnlyList<AwsLoadBalancerInfo> LoadBalancers
    { get; init; } =
        Array.Empty<AwsLoadBalancerInfo>();

    public IReadOnlyList<AwsListenerInfo> Listeners { get; init; } =
        Array.Empty<AwsListenerInfo>();

    public IReadOnlyList<AwsListenerRuleInfo> ListenerRules
    { get; init; } =
        Array.Empty<AwsListenerRuleInfo>();

    public IReadOnlyList<AwsTargetGroupInfo> TargetGroups
    { get; init; } =
        Array.Empty<AwsTargetGroupInfo>();

    public IReadOnlyList<AwsTargetHealthInfo> TargetHealth
    { get; init; } =
        Array.Empty<AwsTargetHealthInfo>();

    public IReadOnlyList<AwsLoadBalancerAttributeInfo>
        LoadBalancerAttributes
    { get; init; } =
        Array.Empty<AwsLoadBalancerAttributeInfo>();

    public DateTimeOffset DiscoveredAt
    { get; init; } =
        DateTimeOffset.UtcNow;

    public IReadOnlyList<string> Warnings
    { get; init; } =
        Array.Empty<string>();
}
