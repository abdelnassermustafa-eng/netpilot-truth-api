using TruthApi.Models.Aws.LoadBalancing;
using TruthApi.Services.Aws.LoadBalancing.Discoverers;

namespace TruthApi.Services.Aws.LoadBalancing;

/// <summary>
/// Coordinates ELBv2 discovery across one or more AWS Regions.
/// </summary>
public sealed class AwsLoadBalancingDiscoveryService
{
    private readonly AwsClientFactory _clientFactory;
    private readonly LoadBalancerDiscoverer _loadBalancerDiscoverer;
    private readonly LoadBalancerAttributeDiscoverer _loadBalancerAttributeDiscoverer;
    private readonly ListenerDiscoverer _listenerDiscoverer;
    private readonly ListenerRuleDiscoverer _listenerRuleDiscoverer;
    private readonly ListenerCertificateDiscoverer _listenerCertificateDiscoverer;
    private readonly TargetGroupDiscoverer _targetGroupDiscoverer;
    private readonly TargetHealthDiscoverer _targetHealthDiscoverer;

    public AwsLoadBalancingDiscoveryService(
        AwsClientFactory clientFactory,
        LoadBalancerDiscoverer loadBalancerDiscoverer,
        LoadBalancerAttributeDiscoverer loadBalancerAttributeDiscoverer,
        ListenerDiscoverer listenerDiscoverer,
        ListenerRuleDiscoverer listenerRuleDiscoverer,
        ListenerCertificateDiscoverer listenerCertificateDiscoverer,
        TargetGroupDiscoverer targetGroupDiscoverer,
        TargetHealthDiscoverer targetHealthDiscoverer)
    {
        _clientFactory = clientFactory;
        _loadBalancerDiscoverer = loadBalancerDiscoverer;
        _loadBalancerAttributeDiscoverer = loadBalancerAttributeDiscoverer;
        _listenerDiscoverer = listenerDiscoverer;
        _listenerRuleDiscoverer = listenerRuleDiscoverer;
        _listenerCertificateDiscoverer = listenerCertificateDiscoverer;
        _targetGroupDiscoverer = targetGroupDiscoverer;
        _targetHealthDiscoverer = targetHealthDiscoverer;
    }

    public async Task<AwsLoadBalancingInventory> DiscoverAsync(
        IReadOnlyCollection<string>? requestedRegions = null,
        CancellationToken cancellationToken = default)
    {
        var regions = NormalizeRegions(requestedRegions);

        var tasks = regions
            .Select(region =>
                DiscoverRegionAsync(
                    region,
                    cancellationToken))
            .ToArray();

        var regionalResults = await Task.WhenAll(tasks);

        return new AwsLoadBalancingInventory
        {
            LoadBalancers = regionalResults
                .SelectMany(result => result.LoadBalancers)
                .OrderBy(loadBalancer => loadBalancer.Region)
                .ThenBy(loadBalancer => loadBalancer.Type)
                .ThenBy(loadBalancer => loadBalancer.Name)
                .ThenBy(loadBalancer =>
                    loadBalancer.LoadBalancerArn)
                .ToList(),
            Listeners = regionalResults
                .SelectMany(result => result.Listeners)
                .OrderBy(listener => listener.Region)
                .ThenBy(listener => listener.LoadBalancerArn)
                .ThenBy(listener => listener.Port)
                .ThenBy(listener => listener.Protocol)
                .ThenBy(listener => listener.ListenerArn)
                .ToList(),
            ListenerRules = regionalResults
                .SelectMany(result => result.ListenerRules)
                .OrderBy(rule => rule.Region)
                .ThenBy(rule => rule.ListenerArn)
                .ThenBy(rule => rule.Priority)
                .ThenBy(rule => rule.RuleArn)
                .ToList(),
            TargetGroups = regionalResults
                .SelectMany(result => result.TargetGroups)
                .OrderBy(group => group.Region)
                .ThenBy(group => group.Name)
                .ThenBy(group => group.TargetGroupArn)
                .ToList(),
            TargetHealth = regionalResults
                .SelectMany(result => result.TargetHealth)
                .OrderBy(item => item.Region)
                .ThenBy(item => item.TargetGroupArn)
                .ThenBy(item => item.TargetId)
                .ThenBy(item => item.Port)
                .ToList(),
            LoadBalancerAttributes = regionalResults
                .SelectMany(result => result.LoadBalancerAttributes)
                .OrderBy(attribute => attribute.Region)
                .ThenBy(attribute => attribute.LoadBalancerArn)
                .ThenBy(attribute => attribute.Key)
                .ToList(),
            ListenerCertificates = regionalResults
                .SelectMany(result => result.ListenerCertificates)
                .OrderBy(item => item.Region)
                .ThenBy(item => item.ListenerArn)
                .ThenByDescending(item => item.IsDefault)
                .ThenBy(item => item.CertificateArn)
                .ToList(),
            Warnings = regionalResults
                .SelectMany(result => result.Warnings)
                .ToList(),
            DiscoveredAt = DateTimeOffset.UtcNow
        };
    }

    private IReadOnlyList<string> NormalizeRegions(
        IReadOnlyCollection<string>? requestedRegions)
    {
        var regions = requestedRegions?
            .Where(region =>
                !string.IsNullOrWhiteSpace(region))
            .Select(region => region.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(region => region)
            .ToList();

        return regions is { Count: > 0 }
            ? regions
            : [_clientFactory.DefaultRegion];
    }

    private async Task<RegionalLoadBalancingResult>
        DiscoverRegionAsync(
            string region,
            CancellationToken cancellationToken)
    {
        try
        {
            var client = _clientFactory.GetElbv2Client(region);

            var targetGroupsTask =
                _targetGroupDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var loadBalancers =
                await _loadBalancerDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var loadBalancerAttributesTask =
                _loadBalancerAttributeDiscoverer.DiscoverAsync(
                    client,
                    region,
                    loadBalancers
                        .Select(loadBalancer =>
                            loadBalancer.LoadBalancerArn)
                        .ToList(),
                    cancellationToken);

            var listeners =
                await _listenerDiscoverer.DiscoverAsync(
                    client,
                    region,
                    loadBalancers
                        .Select(loadBalancer =>
                            loadBalancer.LoadBalancerArn)
                        .ToList(),
                    cancellationToken);

            var listenerRulesTask =
                _listenerRuleDiscoverer.DiscoverAsync(
                    client,
                    region,
                    listeners,
                    cancellationToken);

            var listenerCertificatesTask =
                _listenerCertificateDiscoverer.DiscoverAsync(
                    client,
                    region,
                    listeners,
                    cancellationToken);

            await Task.WhenAll(
                listenerRulesTask,
                listenerCertificatesTask);

            var listenerRules = await listenerRulesTask;
            var listenerCertificates =
                await listenerCertificatesTask;

            var targetGroups = await targetGroupsTask;

            var targetHealth =
                await _targetHealthDiscoverer.DiscoverAsync(
                    client,
                    region,
                    targetGroups
                        .Select(group =>
                            group.TargetGroupArn)
                        .ToList(),
                    cancellationToken);

            var loadBalancerAttributes =
                await loadBalancerAttributesTask;

            return new RegionalLoadBalancingResult
            {
                LoadBalancers = loadBalancers,
                LoadBalancerAttributes = loadBalancerAttributes,
                Listeners = listeners,
                ListenerRules = listenerRules,
                ListenerCertificates = listenerCertificates,
                TargetGroups = targetGroups,
                TargetHealth = targetHealth
            };
        }
        catch (Exception exception)
        {
            return new RegionalLoadBalancingResult
            {
                Warnings =
                [
                    $"Region {region} could not be discovered: " +
                    exception.Message
                ]
            };
        }
    }

    private sealed class RegionalLoadBalancingResult
    {
        public IReadOnlyList<AwsLoadBalancerInfo> LoadBalancers
        { get; init; } =
            Array.Empty<AwsLoadBalancerInfo>();

        public IReadOnlyList<AwsListenerInfo> Listeners
        { get; init; } =
            Array.Empty<AwsListenerInfo>();

        public IReadOnlyList<AwsListenerRuleInfo>
            ListenerRules
        { get; init; } =
            Array.Empty<AwsListenerRuleInfo>();

        public IReadOnlyList<AwsTargetGroupInfo> TargetGroups
        { get; init; } =
            Array.Empty<AwsTargetGroupInfo>();

        public IReadOnlyList<AwsTargetHealthInfo> TargetHealth
        { get; init; } =
            Array.Empty<AwsTargetHealthInfo>();

        public IReadOnlyList<AwsLoadBalancerAttributeInfo>
            LoadBalancerAttributes
        { get; init; } =
            Array.Empty<AwsLoadBalancerAttributeInfo>();

        public IReadOnlyList<AwsListenerCertificateInfo>
            ListenerCertificates
        { get; init; } =
            Array.Empty<AwsListenerCertificateInfo>();

        public IReadOnlyList<string> Warnings { get; init; } =
            Array.Empty<string>();
    }
}
