namespace TruthApi.Models.Aws;

/// <summary>
/// Represents the AWS account, identity, and Regions available to TruthApi.
/// </summary>
public sealed class AwsEnvironmentInfo
{
    public AwsIdentityInfo Identity { get; init; } = new();

    public string DefaultRegion { get; init; } = "";

    public IReadOnlyList<AwsRegionInfo> Regions { get; init; } =
        Array.Empty<AwsRegionInfo>();

    public DateTime DiscoveredAtUtc { get; init; } = DateTime.UtcNow;
}
