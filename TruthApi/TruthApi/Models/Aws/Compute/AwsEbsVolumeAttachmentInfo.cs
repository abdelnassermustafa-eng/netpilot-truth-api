namespace TruthApi.Models.Aws.Compute;

public sealed class AwsEbsVolumeAttachmentInfo
{
    public string VolumeId { get; init; } = "";

    public string InstanceId { get; init; } = "";

    public string Device { get; init; } = "";

    public string State { get; init; } = "";

    public bool DeleteOnTermination { get; init; }

    public DateTimeOffset? AttachTime { get; init; }

    public string AssociatedResource { get; init; } = "";

    public string InstanceOwningService { get; init; } = "";
}
