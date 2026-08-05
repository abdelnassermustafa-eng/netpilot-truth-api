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
    private readonly ListenerDiscoverer _listenerDiscoverer;
    private readonly ListenerRuleDiscoverer _listenerRuleDiscoverer;

    public AwsLoadBalancingDiscoveryService(
        AwsClientFactory clientFactory,
        LoadBalancerDiscoverer loadBalancerDiscoverer,
        ListenerDiscoverer listenerDiscoverer,
        ListenerRuleDiscoverer listenerRuleDiscoverer)
    {
        _clientFactory = clientFactory;
        _loadBalancerDiscoverer = loadBalancerDiscoverer;
        _listenerDiscoverer = listenerDiscoverer;
        _listenerRuleDiscoverer = listenerRuleDiscoverer;
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

            var loadBalancers =
                await _loadBalancerDiscoverer.DiscoverAsync(
                    client,
                    region,
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

            var listenerRules =
                await _listenerRuleDiscoverer.DiscoverAsync(
                    client,
                    region,
                    listeners,
                    cancellationToken);

            return new RegionalLoadBalancingResult
            {
                LoadBalancers = loadBalancers,
                Listeners = listeners,
                ListenerRules = listenerRules
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

        public IReadOnlyList<string> Warnings { get; init; } =
            Array.Empty<string>();
    }
}
