namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents an Auto Scaling group metric enabled for publication
/// to Amazon CloudWatch.
/// </summary>
public sealed class AwsAutoScalingEnabledMetricInfo
{
    public string Metric { get; init; } = "";

    public string Granularity { get; init; } = "";
}
