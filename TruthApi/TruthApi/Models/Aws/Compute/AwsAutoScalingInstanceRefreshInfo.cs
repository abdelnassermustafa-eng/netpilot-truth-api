namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents a recent EC2 Auto Scaling instance-refresh operation.
/// </summary>
public sealed class AwsAutoScalingInstanceRefreshInfo
{
    public string InstanceRefreshId { get; init; } = "";

    public string AutoScalingGroupName { get; init; } = "";

    public string Status { get; init; } = "";

    public string StatusReason { get; init; } = "";

    public string Strategy { get; init; } = "";

    public int? PercentageComplete { get; init; }

    public int? InstancesToUpdate { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? EndedAt { get; init; }

    public bool HasDesiredConfiguration { get; init; }

    public bool HasProgressDetails { get; init; }

    public AwsInstanceRefreshPreferencesInfo? Preferences { get; init; }

    public AwsInstanceRefreshRollbackInfo? Rollback { get; init; }

    public bool IsComplete =>
        string.Equals(
            Status,
            "Successful",
            StringComparison.OrdinalIgnoreCase) ||
        string.Equals(
            Status,
            "Failed",
            StringComparison.OrdinalIgnoreCase) ||
        string.Equals(
            Status,
            "Cancelled",
            StringComparison.OrdinalIgnoreCase) ||
        string.Equals(
            Status,
            "RollbackSuccessful",
            StringComparison.OrdinalIgnoreCase) ||
        string.Equals(
            Status,
            "RollbackFailed",
            StringComparison.OrdinalIgnoreCase);

    public string Region { get; init; } = "";
}
