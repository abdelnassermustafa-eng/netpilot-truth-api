namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents the warm-pool configuration and prepared instances for an
/// Auto Scaling group.
/// </summary>
public sealed class AwsAutoScalingWarmPoolInfo
{
    public string AutoScalingGroupName { get; init; } = "";

    public int? MinSize { get; init; }

    public int? MaxGroupPreparedCapacity { get; init; }

    public string PoolState { get; init; } = "";

    public string Status { get; init; } = "";

    public bool ReuseOnScaleIn { get; init; }

    public int InstanceCount => Instances.Count;

    public string Region { get; init; } = "";

    public IReadOnlyList<AwsAutoScalingWarmPoolInstanceInfo> Instances
    { get; init; } =
        Array.Empty<AwsAutoScalingWarmPoolInstanceInfo>();
}
