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

    public DateTimeOffset DiscoveredAt
    { get; init; } =
        DateTimeOffset.UtcNow;

    public IReadOnlyList<string> Warnings
    { get; init; } =
        Array.Empty<string>();
}
