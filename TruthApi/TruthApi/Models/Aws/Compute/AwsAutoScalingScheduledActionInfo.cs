namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents a pending or recurring EC2 Auto Scaling scheduled action.
/// </summary>
public sealed class AwsAutoScalingScheduledActionInfo
{
    public string ScheduledActionName { get; init; } = "";

    public string ScheduledActionArn { get; init; } = "";

    public string AutoScalingGroupName { get; init; } = "";

    public int? MinSize { get; init; }

    public int? MaxSize { get; init; }

    public int? DesiredCapacity { get; init; }

    public DateTimeOffset? StartTime { get; init; }

    public DateTimeOffset? EndTime { get; init; }

    public DateTimeOffset? LegacyTime { get; init; }

    public string Recurrence { get; init; } = "";

    public string TimeZone { get; init; } = "";

    public bool IsRecurring =>
        !string.IsNullOrWhiteSpace(Recurrence);

    public string Region { get; init; } = "";
}
