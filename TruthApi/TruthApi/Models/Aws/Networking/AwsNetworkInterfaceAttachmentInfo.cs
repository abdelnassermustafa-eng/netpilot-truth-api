namespace TruthApi.Models.Aws.Networking;

public sealed class AwsNetworkInterfaceAttachmentInfo
{
    public string AttachmentId { get; init; } = "";

    public string InstanceId { get; init; } = "";

    public string InstanceOwnerId { get; init; } = "";

    public int DeviceIndex { get; init; }

    public int NetworkCardIndex { get; init; }

    public string Status { get; init; } = "";

    public bool DeleteOnTermination { get; init; }

    public DateTimeOffset? AttachTime { get; init; }
}
