namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents an EC2 Auto Scaling policy associated with an
/// Auto Scaling group.
/// </summary>
public sealed class AwsAutoScalingPolicyInfo
{
    public string PolicyName { get; init; } = "";

    public string PolicyArn { get; init; } = "";

    public string AutoScalingGroupName { get; init; } = "";

    public string PolicyType { get; init; } = "";

    public bool Enabled { get; init; }

    public string AdjustmentType { get; init; } = "";

    public int? ScalingAdjustment { get; init; }

    public int? MinAdjustmentMagnitude { get; init; }

    public int? MinAdjustmentStep { get; init; }

    public int? CooldownSeconds { get; init; }

    public int? EstimatedInstanceWarmupSeconds { get; init; }

    public string MetricAggregationType { get; init; } = "";

    public bool HasPredictiveScalingConfiguration { get; init; }

    public string Region { get; init; } = "";

    public AwsTargetTrackingPolicyInfo? TargetTracking { get; init; }

    public IReadOnlyList<AwsScalingPolicyStepAdjustmentInfo>
        StepAdjustments
    { get; init; } =
        Array.Empty<AwsScalingPolicyStepAdjustmentInfo>();

    public IReadOnlyList<AwsScalingPolicyAlarmInfo> Alarms { get; init; } =
        Array.Empty<AwsScalingPolicyAlarmInfo>();
}
