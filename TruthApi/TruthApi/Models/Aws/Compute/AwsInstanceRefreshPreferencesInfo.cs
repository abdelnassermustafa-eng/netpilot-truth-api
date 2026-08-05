namespace TruthApi.Models.Aws.Compute;

public sealed class AwsInstanceRefreshPreferencesInfo
{
    public bool AutoRollback { get; init; }

    public bool SkipMatching { get; init; }

    public int? MinHealthyPercentage { get; init; }

    public int? MaxHealthyPercentage { get; init; }

    public int? InstanceWarmupSeconds { get; init; }

    public int? CheckpointDelaySeconds { get; init; }

    public int? BakeTimeSeconds { get; init; }

    public string ScaleInProtectedInstances { get; init; } = "";

    public string StandbyInstances { get; init; } = "";

    public IReadOnlyList<int> CheckpointPercentages { get; init; } =
        Array.Empty<int>();

    public IReadOnlyList<string> AlarmNames { get; init; } =
        Array.Empty<string>();
}
