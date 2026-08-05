namespace TruthApi.Models.Aws.Networking;

public sealed class AwsSecurityGroupRuleInfo
{
    public string Protocol { get; init; } = "";

    public int? FromPort { get; init; }

    public int? ToPort { get; init; }

    public IReadOnlyList<string> Ipv4Ranges { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> Ipv6Ranges { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> PrefixListIds { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> ReferencedSecurityGroupIds { get; init; } =
        Array.Empty<string>();

    public string Description { get; init; } = "";
}
