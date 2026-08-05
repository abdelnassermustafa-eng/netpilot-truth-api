namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents an Amazon EBS snapshot owned by the current AWS account.
/// </summary>
public sealed class AwsEbsSnapshotInfo
{
    public string SnapshotId { get; init; } = "";

    public string Name { get; init; } = "";

    public string VolumeId { get; init; } = "";

    public string OwnerId { get; init; } = "";

    public string OwnerAlias { get; init; } = "";

    public string State { get; init; } = "";

    public string Description { get; init; } = "";

    public int VolumeSizeGiB { get; init; }

    public int ProgressPercent { get; init; }

    public bool Encrypted { get; init; }

    public string KmsKeyId { get; init; } = "";

    public string DataEncryptionKeyId { get; init; } = "";

    public string StorageTier { get; init; } = "";

    public bool RestoreExpiryTimePresent { get; init; }

    public DateTimeOffset? RestoreExpiryTime { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public string Region { get; init; } = "";

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
