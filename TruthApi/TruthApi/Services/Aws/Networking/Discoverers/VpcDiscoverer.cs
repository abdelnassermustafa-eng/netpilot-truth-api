using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Networking;
using TruthApi.Services.Aws.Networking.Infrastructure;

namespace TruthApi.Services.Aws.Networking.Discoverers;

/// <summary>
/// Discovers all VPCs in one AWS Region.
/// </summary>
public sealed class VpcDiscoverer
{
    public async Task<IReadOnlyList<AwsVpcInfo>> DiscoverAsync(
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

        var results = new List<AwsVpcInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeVpcsAsync(
                new DescribeVpcsRequest
                {
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var vpc in response.Vpcs ?? [])
            {
                var tags = AwsTagHelper.ToDictionary(vpc.Tags);

                results.Add(new AwsVpcInfo
                {
                    VpcId = vpc.VpcId ?? "",
                    Name = AwsTagHelper.GetName(tags),
                    CidrBlock = vpc.CidrBlock ?? "",
                    State = vpc.State?.Value ?? "",
                    IsDefault = vpc.IsDefault ?? false,
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
