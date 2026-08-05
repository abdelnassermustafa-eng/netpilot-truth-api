namespace TruthApi.Models.Aws.LoadBalancing;

/// <summary>
/// Represents an ELBv2 listener rule, its conditions, and ordered actions.
/// </summary>
public sealed class AwsListenerRuleInfo
{
    public string RuleArn { get; init; } = "";

    public string ListenerArn { get; init; } = "";

    public string Priority { get; init; } = "";

    public bool IsDefault { get; init; }

    public string Region { get; init; } = "";

    public IReadOnlyList<AwsListenerRuleConditionInfo> Conditions
    { get; init; } =
        Array.Empty<AwsListenerRuleConditionInfo>();

    public IReadOnlyList<AwsListenerActionInfo> Actions { get; init; } =
        Array.Empty<AwsListenerActionInfo>();

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
