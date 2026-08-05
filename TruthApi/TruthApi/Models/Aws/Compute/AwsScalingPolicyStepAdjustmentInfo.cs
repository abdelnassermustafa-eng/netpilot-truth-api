namespace TruthApi.Models.Aws.Compute;

public sealed class AwsScalingPolicyStepAdjustmentInfo
{
    public double? MetricIntervalLowerBound { get; init; }

    public double? MetricIntervalUpperBound { get; init; }

    public int? ScalingAdjustment { get; init; }
}
