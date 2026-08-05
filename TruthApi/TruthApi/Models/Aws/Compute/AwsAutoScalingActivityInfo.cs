namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents a recent EC2 Auto Scaling activity, including its status,
/// progress, cause, timing, and diagnostic details.
/// </summary>
public sealed class AwsAutoScalingActivityInfo
{
    public string ActivityId { get; init; } = "";

    public string AutoScalingGroupName { get; init; } = "";

    public string AutoScalingGroupArn { get; init; } = "";

    public string AutoScalingGroupState { get; init; } = "";

    public string Description { get; init; } = "";

    public string Cause { get; init; } = "";

    public string Details { get; init; } = "";

    public string StatusCode { get; init; } = "";

    public string StatusMessage { get; init; } = "";

    public int ProgressPercent { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? EndedAt { get; init; }

    public bool IsComplete =>
        EndedAt.HasValue ||
        string.Equals(
            StatusCode,
            "Successful",
            StringComparison.OrdinalIgnoreCase) ||
        string.Equals(
            StatusCode,
            "Failed",
            StringComparison.OrdinalIgnoreCase) ||
        string.Equals(
            StatusCode,
            "Cancelled",
            StringComparison.OrdinalIgnoreCase);

    public string Region { get; init; } = "";
}
