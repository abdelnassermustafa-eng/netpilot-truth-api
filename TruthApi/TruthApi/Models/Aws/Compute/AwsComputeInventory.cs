namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents live AWS compute inventory for one account and one or more
/// selected Regions.
/// </summary>
public sealed class AwsComputeInventory
{
    public string AccountId { get; init; } = "";

    public IReadOnlyList<string> Regions { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<AwsEc2InstanceInfo> Instances { get; init; } =
        Array.Empty<AwsEc2InstanceInfo>();

    public IReadOnlyList<AwsEbsVolumeInfo> Volumes { get; init; } =
        Array.Empty<AwsEbsVolumeInfo>();

    public IReadOnlyList<AwsEbsSnapshotInfo> Snapshots { get; init; } =
        Array.Empty<AwsEbsSnapshotInfo>();

    public IReadOnlyList<AwsAmiInfo> Images { get; init; } =
        Array.Empty<AwsAmiInfo>();

    public IReadOnlyList<AwsEc2KeyPairInfo> KeyPairs { get; init; } =
        Array.Empty<AwsEc2KeyPairInfo>();

    public IReadOnlyList<AwsLaunchTemplateInfo> LaunchTemplates
    { get; init; } =
        Array.Empty<AwsLaunchTemplateInfo>();

    public IReadOnlyList<AwsAutoScalingGroupInfo> AutoScalingGroups
    { get; init; } =
        Array.Empty<AwsAutoScalingGroupInfo>();

    public IReadOnlyList<AwsLaunchConfigurationInfo>
        LaunchConfigurations
    { get; init; } =
        Array.Empty<AwsLaunchConfigurationInfo>();

    public IReadOnlyList<AwsAutoScalingPolicyInfo>
        AutoScalingPolicies
    { get; init; } =
        Array.Empty<AwsAutoScalingPolicyInfo>();

    public IReadOnlyList<
        AwsAutoScalingScheduledActionInfo> ScheduledActions
    { get; init; } =
        Array.Empty<AwsAutoScalingScheduledActionInfo>();

    public IReadOnlyList<AwsAutoScalingActivityInfo>
        AutoScalingActivities
    { get; init; } =
        Array.Empty<AwsAutoScalingActivityInfo>();

    public IReadOnlyList<
        AwsAutoScalingLifecycleHookInfo> LifecycleHooks
    { get; init; } =
        Array.Empty<AwsAutoScalingLifecycleHookInfo>();

    public IReadOnlyList<AwsAutoScalingWarmPoolInfo>
        WarmPools
    { get; init; } =
        Array.Empty<AwsAutoScalingWarmPoolInfo>();

    public IReadOnlyList<
        AwsAutoScalingInstanceRefreshInfo> InstanceRefreshes
    { get; init; } =
        Array.Empty<AwsAutoScalingInstanceRefreshInfo>();

    public DateTimeOffset DiscoveredAt { get; init; } =
        DateTimeOffset.UtcNow;

    public IReadOnlyList<string> Warnings { get; init; } =
        Array.Empty<string>();
}
