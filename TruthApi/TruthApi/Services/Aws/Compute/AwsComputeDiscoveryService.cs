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
    private readonly EbsVolumeDiscoverer _volumeDiscoverer;

    public AwsComputeDiscoveryService(
        AwsClientFactory clientFactory,
        AwsIdentityService identityService,
        Ec2InstanceDiscoverer instanceDiscoverer,
        EbsVolumeDiscoverer volumeDiscoverer)
    {
        _clientFactory = clientFactory;
        _identityService = identityService;
        _instanceDiscoverer = instanceDiscoverer;
        _volumeDiscoverer = volumeDiscoverer;
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
            Volumes = regionalResults
                .SelectMany(result => result.Volumes)
                .OrderBy(volume => volume.Region)
                .ThenBy(volume => volume.Name)
                .ThenBy(volume => volume.VolumeId)
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

            var instancesTask =
                _instanceDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var volumesTask =
                _volumeDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            await Task.WhenAll(
                instancesTask,
                volumesTask);

            return new RegionalComputeResult
            {
                Instances = await instancesTask,
                Volumes = await volumesTask
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

        public IReadOnlyList<AwsEbsVolumeInfo> Volumes { get; init; } =
            Array.Empty<AwsEbsVolumeInfo>();

        public IReadOnlyList<string> Warnings { get; init; } =
            Array.Empty<string>();
    }
}
