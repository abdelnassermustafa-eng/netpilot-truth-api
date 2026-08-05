namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents a live Amazon EBS volume and its attachments.
/// </summary>
public sealed class AwsEbsVolumeInfo
{
    public string VolumeId { get; init; } = "";

    public string Name { get; init; } = "";

    public string VolumeType { get; init; } = "";

    public string State { get; init; } = "";

    public int SizeGiB { get; init; }

    public int? Iops { get; init; }

    public int? ThroughputMiBps { get; init; }

    public bool Encrypted { get; init; }

    public string KmsKeyId { get; init; } = "";

    public string SnapshotId { get; init; } = "";

    public bool MultiAttachEnabled { get; init; }

    public bool FastRestored { get; init; }

    public string AvailabilityZone { get; init; } = "";

    public string AvailabilityZoneId { get; init; } = "";

    public DateTimeOffset? CreatedAt { get; init; }

    public string Region { get; init; } = "";

    public IReadOnlyList<AwsEbsVolumeAttachmentInfo> Attachments
    { get; init; } =
        Array.Empty<AwsEbsVolumeAttachmentInfo>();

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
