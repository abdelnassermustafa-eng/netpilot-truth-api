namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents an EC2 Launch Template and selected version details.
/// </summary>
public sealed class AwsLaunchTemplateInfo
{
    public string LaunchTemplateId { get; init; } = "";

    public string LaunchTemplateName { get; init; } = "";

    public string CreatedBy { get; init; } = "";

    public DateTimeOffset? CreatedAt { get; init; }

    public long DefaultVersionNumber { get; init; }

    public long LatestVersionNumber { get; init; }

    public string Region { get; init; } = "";

    public AwsLaunchTemplateVersionInfo? DefaultVersion { get; init; }

    public AwsLaunchTemplateVersionInfo? LatestVersion { get; init; }

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
