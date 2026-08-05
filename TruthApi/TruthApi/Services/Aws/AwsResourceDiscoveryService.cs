using TruthApi.Models.Aws;

namespace TruthApi.Services.Aws;

/// <summary>
/// Coordinates AWS identity, Region, and future resource discovery.
///
/// Future networking, compute, storage, identity, cost, and protection
/// discovery services will be orchestrated through this class.
/// </summary>
public sealed class AwsResourceDiscoveryService
{
    private readonly AwsClientFactory _clientFactory;
    private readonly AwsIdentityService _identityService;
    private readonly AwsRegionService _regionService;

    public AwsResourceDiscoveryService(
        AwsClientFactory clientFactory,
        AwsIdentityService identityService,
        AwsRegionService regionService)
    {
        _clientFactory = clientFactory;
        _identityService = identityService;
        _regionService = regionService;
    }

    public async Task<AwsEnvironmentInfo> DiscoverEnvironmentAsync(
        bool includeDisabledRegions = true,
        CancellationToken cancellationToken = default)
    {
        var identityTask =
            _identityService.GetCurrentIdentityAsync(cancellationToken);

        var regionsTask =
            _regionService.GetRegionsAsync(
                includeDisabledRegions,
                cancellationToken);

        await Task.WhenAll(identityTask, regionsTask);

        return new AwsEnvironmentInfo
        {
            Identity = await identityTask,
            DefaultRegion = _clientFactory.DefaultRegion,
            Regions = await regionsTask,
            DiscoveredAtUtc = DateTime.UtcNow
        };
    }
}
