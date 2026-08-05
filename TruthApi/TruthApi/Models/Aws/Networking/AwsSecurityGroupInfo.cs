namespace TruthApi.Models.Aws.Networking;

public sealed class AwsSecurityGroupInfo
{
    public string GroupId { get; init; } = "";

    public string GroupName { get; init; } = "";

    public string Name { get; init; } = "";

    public string Description { get; init; } = "";

    public string VpcId { get; init; } = "";

    public string OwnerId { get; init; } = "";

    public string Region { get; init; } = "";

    public IReadOnlyList<AwsSecurityGroupRuleInfo> IngressRules
    { get; init; } =
        Array.Empty<AwsSecurityGroupRuleInfo>();

    public IReadOnlyList<AwsSecurityGroupRuleInfo> EgressRules
    { get; init; } =
        Array.Empty<AwsSecurityGroupRuleInfo>();

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
