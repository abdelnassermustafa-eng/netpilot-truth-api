using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using TruthApi.Models.Aws.LoadBalancing;
using Elbv2Action = Amazon.ElasticLoadBalancingV2.Model.Action;

namespace TruthApi.Services.Aws.LoadBalancing.Discoverers;

/// <summary>
/// Discovers listener rules for HTTP and HTTPS ELBv2 listeners.
/// </summary>
public sealed class ListenerRuleDiscoverer
{
    public async Task<IReadOnlyList<AwsListenerRuleInfo>> DiscoverAsync(
        IAmazonElasticLoadBalancingV2 client,
        string region,
        IReadOnlyCollection<AwsListenerInfo> listeners,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(listeners);

        if (string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException(
                "AWS Region is required.",
                nameof(region));
        }

        // Listener rules apply to Application Load Balancer HTTP/HTTPS
        // listeners. Avoid invalid DescribeRules calls for NLB/GWLB
        // protocols such as TCP, TLS, UDP, TCP_UDP, and GENEVE.
        var eligibleListeners = listeners
            .Where(listener =>
                string.Equals(
                    listener.Protocol,
                    "HTTP",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    listener.Protocol,
                    "HTTPS",
                    StringComparison.OrdinalIgnoreCase))
            .Where(listener =>
                !string.IsNullOrWhiteSpace(listener.ListenerArn))
            .ToList();

        if (eligibleListeners.Count == 0)
        {
            return Array.Empty<AwsListenerRuleInfo>();
        }

        var tasks = eligibleListeners
            .Select(listener =>
                DiscoverForListenerAsync(
                    client,
                    region,
                    listener.ListenerArn,
                    cancellationToken))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        var rules = results
            .SelectMany(result => result)
            .ToList();

        var tagsByArn = await GetTagsByArnAsync(
            client,
            rules
                .Select(rule => rule.RuleArn)
                .Where(arn => !string.IsNullOrWhiteSpace(arn))
                .ToList(),
            cancellationToken);

        return rules
            .Select(rule =>
            {
                tagsByArn.TryGetValue(rule.RuleArn, out var tags);

                return new AwsListenerRuleInfo
                {
                    RuleArn = rule.RuleArn,
                    ListenerArn = rule.ListenerArn,
                    Priority = rule.Priority,
                    IsDefault = rule.IsDefault,
                    Region = rule.Region,
                    Conditions = rule.Conditions,
                    Actions = rule.Actions,
                    Tags = tags ??
                        new Dictionary<string, string>(
                            StringComparer.OrdinalIgnoreCase)
                };
            })
            .OrderBy(rule => rule.ListenerArn)
            .ThenBy(rule => PrioritySortValue(rule.Priority))
            .ThenBy(rule => rule.RuleArn)
            .ToList();
    }

    private static async Task<IReadOnlyList<AwsListenerRuleInfo>>
        DiscoverForListenerAsync(
            IAmazonElasticLoadBalancingV2 client,
            string region,
            string listenerArn,
            CancellationToken cancellationToken)
    {
        var results = new List<AwsListenerRuleInfo>();
        string? marker = null;

        do
        {
            var response = await client.DescribeRulesAsync(
                new DescribeRulesRequest
                {
                    ListenerArn = listenerArn,
                    Marker = marker,
                    PageSize = 100
                },
                cancellationToken);

            foreach (var rule in response.Rules ?? [])
            {
                results.Add(new AwsListenerRuleInfo
                {
                    RuleArn = rule.RuleArn ?? "",
                    ListenerArn = listenerArn,
                    Priority = rule.Priority ?? "",
                    IsDefault = rule.IsDefault ?? false,
                    Region = region,
                    Conditions = (rule.Conditions ?? [])
                        .Select(ToConditionInfo)
                        .ToList(),
                    Actions = (rule.Actions ?? [])
                        .Select(ToActionInfo)
                        .OrderBy(action => action.Order)
                        .ToList()
                });
            }

            marker = response.NextMarker;
        }
        while (!string.IsNullOrWhiteSpace(marker));

        return results;
    }

    private static AwsListenerRuleConditionInfo ToConditionInfo(
        RuleCondition condition)
    {
        var values = new List<string>();
        var regexValues = new List<string>();

        values.AddRange(condition.Values ?? []);
        values.AddRange(condition.HostHeaderConfig?.Values ?? []);
        values.AddRange(condition.PathPatternConfig?.Values ?? []);
        values.AddRange(
            condition.HttpRequestMethodConfig?.Values ?? []);
        values.AddRange(condition.SourceIpConfig?.Values ?? []);
        values.AddRange(condition.HttpHeaderConfig?.Values ?? []);

        regexValues.AddRange(
            condition.HostHeaderConfig?.RegexValues ?? []);
        regexValues.AddRange(
            condition.PathPatternConfig?.RegexValues ?? []);
        regexValues.AddRange(
            condition.HttpHeaderConfig?.RegexValues ?? []);

        return new AwsListenerRuleConditionInfo
        {
            Field = condition.Field ?? "",
            HttpHeaderName =
                condition.HttpHeaderConfig?.HttpHeaderName ?? "",
            Values = values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            RegexValues = regexValues
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToList(),
            QueryStrings =
                (condition.QueryStringConfig?.Values ?? [])
                    .Select(item =>
                        new AwsListenerRuleQueryStringInfo
                        {
                            Key = item.Key ?? "",
                            Value = item.Value ?? ""
                        })
                    .ToList()
        };
    }

    private static AwsListenerActionInfo ToActionInfo(
        Elbv2Action action)
    {
        return new AwsListenerActionInfo
        {
            Order = action.Order,
            Type = action.Type?.Value ?? "",
            TargetGroupArn = action.TargetGroupArn ?? "",

            ForwardTargetGroups =
                (action.ForwardConfig?.TargetGroups ?? [])
                    .Select(group =>
                        new AwsForwardTargetGroupInfo
                        {
                            TargetGroupArn =
                                group.TargetGroupArn ?? "",
                            Weight = group.Weight
                        })
                    .ToList(),

            ForwardStickinessEnabled =
                action.ForwardConfig?
                    .TargetGroupStickinessConfig?
                    .Enabled ?? false,

            ForwardStickinessDurationSeconds =
                action.ForwardConfig?
                    .TargetGroupStickinessConfig?
                    .DurationSeconds,

            RedirectProtocol =
                action.RedirectConfig?.Protocol ?? "",
            RedirectHost =
                action.RedirectConfig?.Host ?? "",
            RedirectPort =
                action.RedirectConfig?.Port ?? "",
            RedirectPath =
                action.RedirectConfig?.Path ?? "",
            RedirectQuery =
                action.RedirectConfig?.Query ?? "",
            RedirectStatusCode =
                action.RedirectConfig?.StatusCode?.Value ?? "",

            FixedResponseContentType =
                action.FixedResponseConfig?.ContentType ?? "",
            FixedResponseMessageBody =
                action.FixedResponseConfig?.MessageBody ?? "",
            FixedResponseStatusCode =
                action.FixedResponseConfig?.StatusCode ?? "",

            AuthenticateOidcIssuer =
                action.AuthenticateOidcConfig?.Issuer ?? "",
            AuthenticateOidcClientId =
                action.AuthenticateOidcConfig?.ClientId ?? "",

            AuthenticateCognitoUserPoolArn =
                action.AuthenticateCognitoConfig?
                    .UserPoolArn ?? "",
            AuthenticateCognitoUserPoolClientId =
                action.AuthenticateCognitoConfig?
                    .UserPoolClientId ?? "",
            AuthenticateCognitoUserPoolDomain =
                action.AuthenticateCognitoConfig?
                    .UserPoolDomain ?? ""
        };
    }

    private static async Task<
        IReadOnlyDictionary<
            string,
            IReadOnlyDictionary<string, string>>>
        GetTagsByArnAsync(
            IAmazonElasticLoadBalancingV2 client,
            IReadOnlyList<string> resourceArns,
            CancellationToken cancellationToken)
    {
        var results = new Dictionary<
            string,
            IReadOnlyDictionary<string, string>>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var batch in resourceArns.Chunk(20))
        {
            var response = await client.DescribeTagsAsync(
                new DescribeTagsRequest
                {
                    ResourceArns = batch.ToList()
                },
                cancellationToken);

            foreach (var description in
                     response.TagDescriptions ?? [])
            {
                var arn = description.ResourceArn ?? "";

                if (string.IsNullOrWhiteSpace(arn))
                {
                    continue;
                }

                results[arn] = (description.Tags ?? [])
                    .Where(tag =>
                        !string.IsNullOrWhiteSpace(tag.Key))
                    .GroupBy(
                        tag => tag.Key,
                        StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Last().Value ?? "",
                        StringComparer.OrdinalIgnoreCase);
            }
        }

        return results;
    }

    private static int PrioritySortValue(string priority)
    {
        return int.TryParse(priority, out var value)
            ? value
            : int.MaxValue;
    }
}
