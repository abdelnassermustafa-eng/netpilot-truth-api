namespace TruthApi.Models.Aws.Compute;

public sealed class AwsInstanceBlockDeviceInfo
{
    public string DeviceName { get; init; } = "";

    public string VolumeId { get; init; } = "";

    public string Status { get; init; } = "";

    public bool DeleteOnTermination { get; init; }

    public DateTimeOffset? AttachTime { get; init; }
}
