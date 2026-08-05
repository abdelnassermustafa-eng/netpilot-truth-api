namespace TruthApi.Models.Aws.Compute;

public sealed class AwsTargetTrackingPolicyInfo
{
    public double? TargetValue { get; init; }

    public bool DisableScaleIn { get; init; }

    public string PredefinedMetricType { get; init; } = "";

    public string ResourceLabel { get; init; } = "";

    public bool UsesCustomizedMetric { get; init; }
}
