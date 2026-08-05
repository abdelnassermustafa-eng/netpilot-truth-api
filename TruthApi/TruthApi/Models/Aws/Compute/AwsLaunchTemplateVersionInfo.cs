namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents a selected version of an EC2 Launch Template.
/// </summary>
public sealed class AwsLaunchTemplateVersionInfo
{
    public long VersionNumber { get; init; }

    public string VersionDescription { get; init; } = "";

    public bool IsDefaultVersion { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string CreatedBy { get; init; } = "";

    public string ImageId { get; init; } = "";

    public string InstanceType { get; init; } = "";

    public string KeyName { get; init; } = "";

    public string IamInstanceProfileArn { get; init; } = "";

    public string IamInstanceProfileName { get; init; } = "";

    public bool HasUserData { get; init; }

    public bool EbsOptimized { get; init; }

    public bool DisableApiTermination { get; init; }

    public string InstanceInitiatedShutdownBehavior { get; init; } = "";

    public string MetadataHttpTokens { get; init; } = "";

    public string MetadataHttpEndpoint { get; init; } = "";

    public int? MetadataHttpPutResponseHopLimit { get; init; }

    public IReadOnlyList<string> SecurityGroupIds { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> SecurityGroupNames { get; init; } =
        Array.Empty<string>();
}
