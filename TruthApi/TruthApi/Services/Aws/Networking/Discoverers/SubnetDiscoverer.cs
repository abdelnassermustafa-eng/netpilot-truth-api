using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Networking;
using TruthApi.Services.Aws.Networking.Infrastructure;

namespace TruthApi.Services.Aws.Networking.Discoverers;

/// <summary>
/// Discovers all subnets in one AWS Region.
/// </summary>
public sealed class SubnetDiscoverer
{
    public async Task<IReadOnlyList<AwsSubnetInfo>> DiscoverAsync(
        IAmazonEC2 client,
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

        var results = new List<AwsSubnetInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeSubnetsAsync(
                new DescribeSubnetsRequest
                {
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var subnet in response.Subnets ?? [])
            {
                var tags = AwsTagHelper.ToDictionary(subnet.Tags);

                results.Add(new AwsSubnetInfo
                {
                    SubnetId = subnet.SubnetId ?? "",
                    Name = AwsTagHelper.GetName(tags),
                    VpcId = subnet.VpcId ?? "",
                    CidrBlock = subnet.CidrBlock ?? "",
                    AvailabilityZone = subnet.AvailabilityZone ?? "",
                    AvailabilityZoneId =
                        subnet.AvailabilityZoneId ?? "",
                    State = subnet.State?.Value ?? "",
                    MapPublicIpOnLaunch =
                        subnet.MapPublicIpOnLaunch ?? false,
                    AvailableIpAddressCount =
                        subnet.AvailableIpAddressCount ?? 0,
                    Region = region,
                    Tags = tags
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }
}
