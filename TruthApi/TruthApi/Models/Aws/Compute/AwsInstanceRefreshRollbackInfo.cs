namespace TruthApi.Models.Aws.Compute;

public sealed class AwsInstanceRefreshRollbackInfo
{
    public int? InstancesToUpdate { get; init; }

    public int? PercentageComplete { get; init; }

    public string Reason { get; init; } = "";

    public DateTimeOffset? StartedAt { get; init; }
}
