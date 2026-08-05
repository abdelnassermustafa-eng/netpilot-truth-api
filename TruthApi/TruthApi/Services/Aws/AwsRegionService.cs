using Amazon.EC2.Model;
using TruthApi.Models.Aws;

namespace TruthApi.Services.Aws;

/// <summary>
/// Discovers AWS Regions visible to the current AWS identity.
/// </summary>
public sealed class AwsRegionService
{
    private readonly AwsClientFactory _clientFactory;

    public AwsRegionService(AwsClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<IReadOnlyList<AwsRegionInfo>> GetRegionsAsync(
        bool includeDisabledRegions = true,
        CancellationToken cancellationToken = default)
    {
        var client = _clientFactory.GetEc2Client();

        var response = await client.DescribeRegionsAsync(
            new DescribeRegionsRequest
            {
                AllRegions = true
            },
            cancellationToken);

        var regions = (response.Regions ?? [])
            .Select(region =>
            {
                var optInStatus = region.OptInStatus ?? "";

                return new AwsRegionInfo
                {
                    RegionName = region.RegionName ?? "",
                    Endpoint = region.Endpoint ?? "",
                    OptInStatus = optInStatus,
                    IsDefaultRegion = string.Equals(
                        region.RegionName,
                        _clientFactory.DefaultRegion,
                        StringComparison.OrdinalIgnoreCase)
                };
            })
            .Where(region =>
                includeDisabledRegions || region.IsEnabled)
            .OrderBy(region => region.RegionName)
            .ToList();

        return regions;
    }
}
