namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents an EC2 Auto Scaling group and its current member instances.
/// </summary>
public sealed class AwsAutoScalingGroupInfo
{
    public string AutoScalingGroupName { get; init; } = "";

    public string AutoScalingGroupArn { get; init; } = "";

    public string Status { get; init; } = "";

    public int MinSize { get; init; }

    public int MaxSize { get; init; }

    public int DesiredCapacity { get; init; }

    public string DesiredCapacityType { get; init; } = "";

    public int DefaultCooldownSeconds { get; init; }

    public int DefaultInstanceWarmupSeconds { get; init; }

    public string HealthCheckType { get; init; } = "";

    public int HealthCheckGracePeriodSeconds { get; init; }

    public bool CapacityRebalance { get; init; }

    public bool NewInstancesProtectedFromScaleIn { get; init; }

    public int? MaxInstanceLifetimeSeconds { get; init; }

    public string PlacementGroup { get; init; } = "";

    public string ServiceLinkedRoleArn { get; init; } = "";

    public string LaunchConfigurationName { get; init; } = "";

    public string LaunchTemplateId { get; init; } = "";

    public string LaunchTemplateName { get; init; } = "";

    public string LaunchTemplateVersion { get; init; } = "";

    public bool UsesMixedInstancesPolicy { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string Region { get; init; } = "";

    public IReadOnlyList<string> AvailabilityZones { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> SubnetIds { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> LoadBalancerNames { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> TargetGroupArns { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> TerminationPolicies { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> SuspendedProcesses { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<AwsAutoScalingEnabledMetricInfo>
        EnabledMetrics
    { get; init; } =
        Array.Empty<AwsAutoScalingEnabledMetricInfo>();

    public IReadOnlyList<AwsAutoScalingInstanceInfo> Instances
    { get; init; } =
        Array.Empty<AwsAutoScalingInstanceInfo>();

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();

    public IReadOnlyDictionary<string, bool> TagPropagation
    { get; init; } =
        new Dictionary<string, bool>();
}
