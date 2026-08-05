namespace TruthApi.Models.Aws.LoadBalancing;

/// <summary>
/// Represents the live registration and health state of one target in an
/// ELBv2 target group.
/// </summary>
public sealed class AwsTargetHealthInfo
{
    public string TargetGroupArn { get; init; } = "";

    public string TargetId { get; init; } = "";

    public int? Port { get; init; }

    public string AvailabilityZone { get; init; } = "";

    public string HealthState { get; init; } = "";

    public string HealthReason { get; init; } = "";

    public string HealthDescription { get; init; } = "";

    public string AdministrativeOverrideState { get; init; } = "";

    public string AdministrativeOverrideReason { get; init; } = "";

    public string AdministrativeOverrideDescription { get; init; } = "";

    public string AnomalyResult { get; init; } = "";

    public string AnomalyMitigationInEffect { get; init; } = "";

    public string Region { get; init; } = "";
}
