namespace TruthApi.Models.Aws;

/// <summary>
/// Describes an AWS Region that is visible to the current identity.
/// </summary>
public sealed class AwsRegionInfo
{
    public string RegionName { get; init; } = "";

    public string Endpoint { get; init; } = "";

    public string OptInStatus { get; init; } = "";

    public bool IsDefaultRegion { get; init; }

    public bool IsEnabled =>
        string.IsNullOrWhiteSpace(OptInStatus) ||
        OptInStatus.Equals("opt-in-not-required", StringComparison.OrdinalIgnoreCase) ||
        OptInStatus.Equals("opted-in", StringComparison.OrdinalIgnoreCase);
}
