namespace TruthApi.Models.Aws.Compute;

public sealed class AwsAmiBlockDeviceInfo
{
    public string DeviceName { get; init; } = "";

    public string VirtualName { get; init; } = "";

    public string NoDevice { get; init; } = "";

    public string SnapshotId { get; init; } = "";

    public int? VolumeSizeGiB { get; init; }

    public string VolumeType { get; init; } = "";

    public int? Iops { get; init; }

    public int? ThroughputMiBps { get; init; }

    public bool Encrypted { get; init; }

    public bool DeleteOnTermination { get; init; }
}
