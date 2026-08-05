namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents an EC2 Auto Scaling lifecycle hook associated with an
/// Auto Scaling group.
/// </summary>
public sealed class AwsAutoScalingLifecycleHookInfo
{
    public string LifecycleHookName { get; init; } = "";

    public string AutoScalingGroupName { get; init; } = "";

    public string LifecycleTransition { get; init; } = "";

    public string DefaultResult { get; init; } = "";

    public int? HeartbeatTimeoutSeconds { get; init; }

    public int? GlobalTimeoutSeconds { get; init; }

    public string NotificationTargetArn { get; init; } = "";

    public string RoleArn { get; init; } = "";

    public string NotificationMetadata { get; init; } = "";

    public bool IsLaunchHook =>
        string.Equals(
            LifecycleTransition,
            "autoscaling:EC2_INSTANCE_LAUNCHING",
            StringComparison.OrdinalIgnoreCase);

    public bool IsTerminationHook =>
        string.Equals(
            LifecycleTransition,
            "autoscaling:EC2_INSTANCE_TERMINATING",
            StringComparison.OrdinalIgnoreCase);

    public string Region { get; init; } = "";
}
