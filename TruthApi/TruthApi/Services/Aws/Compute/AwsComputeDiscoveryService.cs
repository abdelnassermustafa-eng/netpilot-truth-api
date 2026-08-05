using TruthApi.Models.Aws.Compute;
using TruthApi.Services.Aws.Compute.Discoverers.Core;

namespace TruthApi.Services.Aws.Compute;

/// <summary>
/// Coordinates live AWS compute discovery across one or more Regions.
/// </summary>
public sealed class AwsComputeDiscoveryService
{
    private readonly AwsClientFactory _clientFactory;
    private readonly AwsIdentityService _identityService;
    private readonly Ec2InstanceDiscoverer _instanceDiscoverer;

    public AwsComputeDiscoveryService(
        AwsClientFactory clientFactory,
        AwsIdentityService identityService,
        Ec2InstanceDiscoverer instanceDiscoverer)
    {
        _clientFactory = clientFactory;
        _identityService = identityService;
        _instanceDiscoverer = instanceDiscoverer;
    }

    public async Task<AwsComputeInventory> DiscoverAsync(
        IReadOnlyCollection<string>? requestedRegions = null,
        CancellationToken cancellationToken = default)
    {
        var identity = await _identityService.GetCurrentIdentityAsync(
            cancellationToken);

        var regions = NormalizeRegions(requestedRegions);

        var regionalTasks = regions
            .Select(region =>
                DiscoverRegionAsync(region, cancellationToken))
            .ToArray();

        var regionalResults = await Task.WhenAll(regionalTasks);

        return new AwsComputeInventory
        {
            AccountId = identity.AccountId,
            Regions = regions,
            Instances = regionalResults
                .SelectMany(result => result.Instances)
                .OrderBy(instance => instance.Region)
                .ThenBy(instance => instance.Name)
                .ThenBy(instance => instance.InstanceId)
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
            .Where(region => !string.IsNullOrWhiteSpace(region))
            .Select(region => region.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(region => region)
            .ToList();

        if (regions is { Count: > 0 })
        {
            return regions;
        }

        return new[] { _clientFactory.DefaultRegion };
    }

    private async Task<RegionalComputeResult> DiscoverRegionAsync(
        string region,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _clientFactory.GetEc2Client(region);

            return new RegionalComputeResult
            {
                Instances = await _instanceDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken)
            };
        }
        catch (Exception exception)
        {
            return new RegionalComputeResult
            {
                Warnings =
                [
                    $"Region {region} could not be discovered: " +
                    exception.Message
                ]
            };
        }
    }

    private sealed class RegionalComputeResult
    {
        public IReadOnlyList<AwsEc2InstanceInfo> Instances { get; init; } =
            Array.Empty<AwsEc2InstanceInfo>();

        public IReadOnlyList<string> Warnings { get; init; } =
            Array.Empty<string>();
    }
}
