using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using TruthApi.Models.Aws.LoadBalancing;

namespace TruthApi.Services.Aws.LoadBalancing.Discoverers;

/// <summary>
/// Discovers Application, Network, and Gateway Load Balancers
/// in one AWS Region.
/// </summary>
public sealed class LoadBalancerDiscoverer
{
    public async Task<IReadOnlyList<AwsLoadBalancerInfo>> DiscoverAsync(
        IAmazonElasticLoadBalancingV2 client,
        string region,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

        if (string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException(
                "AWS Region is required.",
                nameof(region));
        }

        var loadBalancers = new List<LoadBalancer>();
        string? marker = null;

        do
        {
            var response = await client.DescribeLoadBalancersAsync(
                new DescribeLoadBalancersRequest
                {
                    Marker = marker,
                    PageSize = 100
                },
                cancellationToken);

            loadBalancers.AddRange(response.LoadBalancers ?? []);
            marker = response.NextMarker;
        }
        while (!string.IsNullOrWhiteSpace(marker));

        var tagsByArn = await GetTagsByArnAsync(
            client,
            loadBalancers
                .Select(loadBalancer => loadBalancer.LoadBalancerArn)
                .Where(arn => !string.IsNullOrWhiteSpace(arn))
                .Select(arn => arn!)
                .ToList(),
            cancellationToken);

        return loadBalancers
            .Select(loadBalancer =>
            {
                var arn = loadBalancer.LoadBalancerArn ?? "";

                tagsByArn.TryGetValue(
                    arn,
                    out var tags);

                tags ??= new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

                return new AwsLoadBalancerInfo
                {
                    LoadBalancerArn = arn,
                    Name = loadBalancer.LoadBalancerName ?? "",
                    Type = loadBalancer.Type?.Value ?? "",
                    Scheme = loadBalancer.Scheme?.Value ?? "",
                    State = loadBalancer.State?.Code?.Value ?? "",
                    StateReason =
                        loadBalancer.State?.Reason ?? "",
                    IpAddressType =
                        loadBalancer.IpAddressType?.Value ?? "",
                    VpcId = loadBalancer.VpcId ?? "",
                    DnsName = loadBalancer.DNSName ?? "",
                    CanonicalHostedZoneId =
                        loadBalancer.CanonicalHostedZoneId ?? "",
                    CustomerOwnedIpv4Pool =
                        loadBalancer.CustomerOwnedIpv4Pool ?? "",
                    EnforceSecurityGroupInboundRulesOnPrivateLinkTraffic =
                        string.Equals(
                            loadBalancer
                                .EnforceSecurityGroupInboundRulesOnPrivateLinkTraffic,
                            "on",
                            StringComparison.OrdinalIgnoreCase),
                    CreatedAt = loadBalancer.CreatedTime,
                    Region = region,
                    SecurityGroupIds =
                        (loadBalancer.SecurityGroups ?? [])
                            .Where(id =>
                                !string.IsNullOrWhiteSpace(id))
                            .ToList(),
                    AvailabilityZones =
                        (loadBalancer.AvailabilityZones ?? [])
                            .Select(ToZoneInfo)
                            .OrderBy(zone => zone.ZoneName)
                            .ThenBy(zone => zone.SubnetId)
                            .ToList(),
                    Tags = tags
                };
            })
            .OrderBy(loadBalancer => loadBalancer.Type)
            .ThenBy(loadBalancer => loadBalancer.Name)
            .ThenBy(loadBalancer => loadBalancer.LoadBalancerArn)
            .ToList();
    }

    private static AwsLoadBalancerZoneInfo ToZoneInfo(
        AvailabilityZone zone)
    {
        return new AwsLoadBalancerZoneInfo
        {
            ZoneName = zone.ZoneName ?? "",
            SubnetId = zone.SubnetId ?? "",
            OutpostId = zone.OutpostId ?? "",
            IpAddresses =
                (zone.LoadBalancerAddresses ?? [])
                    .Select(address =>
                        FirstNonEmpty(
                            address.IpAddress,
                            address.PrivateIPv4Address,
                            address.IPv6Address))
                    .Where(value =>
                        !string.IsNullOrWhiteSpace(value))
                    .ToList()
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

    private static string FirstNonEmpty(
        params string?[] values)
    {
        return values.FirstOrDefault(
                   value => !string.IsNullOrWhiteSpace(value))
               ?? "";
    }
}
