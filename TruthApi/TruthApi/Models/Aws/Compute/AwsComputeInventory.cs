namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents live AWS compute inventory for one account and one or more
/// selected Regions.
/// </summary>
public sealed class AwsComputeInventory
{
    public string AccountId { get; init; } = "";

    public IReadOnlyList<string> Regions { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<AwsEc2InstanceInfo> Instances { get; init; } =
        Array.Empty<AwsEc2InstanceInfo>();

    public IReadOnlyList<AwsEbsVolumeInfo> Volumes { get; init; } =
        Array.Empty<AwsEbsVolumeInfo>();

    public IReadOnlyList<AwsEbsSnapshotInfo> Snapshots { get; init; } =
        Array.Empty<AwsEbsSnapshotInfo>();

    public DateTimeOffset DiscoveredAt { get; init; } =
        DateTimeOffset.UtcNow;

    public IReadOnlyList<string> Warnings { get; init; } =
        Array.Empty<string>();
}
