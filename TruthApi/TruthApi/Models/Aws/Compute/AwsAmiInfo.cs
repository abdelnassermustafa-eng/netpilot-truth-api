namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents an Amazon Machine Image owned by the current AWS account.
/// </summary>
public sealed class AwsAmiInfo
{
    public string ImageId { get; init; } = "";

    public string Name { get; init; } = "";

    public string Description { get; init; } = "";

    public string OwnerId { get; init; } = "";

    public string OwnerAlias { get; init; } = "";

    public string State { get; init; } = "";

    public string ImageType { get; init; } = "";

    public string Architecture { get; init; } = "";

    public string Platform { get; init; } = "";

    public string PlatformDetails { get; init; } = "";

    public string VirtualizationType { get; init; } = "";

    public string Hypervisor { get; init; } = "";

    public string RootDeviceType { get; init; } = "";

    public string RootDeviceName { get; init; } = "";

    public string BootMode { get; init; } = "";

    public string ImdsSupport { get; init; } = "";

    public string TpmSupport { get; init; } = "";

    public string SourceImageId { get; init; } = "";

    public string SourceImageRegion { get; init; } = "";

    public string SourceInstanceId { get; init; } = "";

    public bool IsPublic { get; init; }

    public bool EnaSupport { get; init; }

    public bool? IsAllowed { get; init; }

    public string DeregistrationProtection { get; init; } = "";

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? DeprecationTime { get; init; }

    public DateTimeOffset? LastLaunchedTime { get; init; }

    public string Region { get; init; } = "";

    public IReadOnlyList<AwsAmiBlockDeviceInfo> BlockDevices
    { get; init; } =
        Array.Empty<AwsAmiBlockDeviceInfo>();

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
