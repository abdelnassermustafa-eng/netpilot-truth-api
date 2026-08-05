namespace TruthApi.Models.Aws;

/// <summary>
/// Represents the AWS identity currently used by TruthApi.
/// </summary>
public sealed class AwsIdentityInfo
{
    public string AccountId { get; init; } = "";

    public string Arn { get; init; } = "";

    public string PrincipalId { get; init; } = "";

    public string DisplayName { get; init; } = "";

    public DateTime DiscoveredAtUtc { get; init; } = DateTime.UtcNow;
}
