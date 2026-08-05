namespace TruthApi.Models.Aws.LoadBalancing;

/// <summary>
/// Represents one condition used to match an Application Load Balancer
/// listener rule.
/// </summary>
public sealed class AwsListenerRuleConditionInfo
{
    public string Field { get; init; } = "";

    public string HttpHeaderName { get; init; } = "";

    public IReadOnlyList<string> Values { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> RegexValues { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<AwsListenerRuleQueryStringInfo> QueryStrings
    { get; init; } =
        Array.Empty<AwsListenerRuleQueryStringInfo>();
}
