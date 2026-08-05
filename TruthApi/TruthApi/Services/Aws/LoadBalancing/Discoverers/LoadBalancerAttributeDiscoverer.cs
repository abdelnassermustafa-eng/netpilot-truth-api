using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using TruthApi.Models.Aws.LoadBalancing;

namespace TruthApi.Services.Aws.LoadBalancing.Discoverers;

/// <summary>
/// Discovers operational attributes for known ELBv2 load balancers
/// in one AWS Region.
/// </summary>
public sealed class LoadBalancerAttributeDiscoverer
{
    public async Task<IReadOnlyList<AwsLoadBalancerAttributeInfo>>
        DiscoverAsync(
            IAmazonElasticLoadBalancingV2 client,
            string region,
            IReadOnlyCollection<string> loadBalancerArns,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(loadBalancerArns);

        if (string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException(
                "AWS Region is required.",
                nameof(region));
        }

        var arns = loadBalancerArns
            .Where(arn => !string.IsNullOrWhiteSpace(arn))
            .Select(arn => arn.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(arn => arn)
            .ToList();

        if (arns.Count == 0)
        {
            return Array.Empty<AwsLoadBalancerAttributeInfo>();
        }

        var tasks = arns
            .Select(arn =>
                DiscoverForLoadBalancerAsync(
                    client,
                    region,
                    arn,
                    cancellationToken))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        return results
            .SelectMany(result => result)
            .OrderBy(attribute => attribute.LoadBalancerArn)
            .ThenBy(attribute => attribute.Key)
            .ToList();
    }

    private static async Task<
        IReadOnlyList<AwsLoadBalancerAttributeInfo>>
        DiscoverForLoadBalancerAsync(
            IAmazonElasticLoadBalancingV2 client,
            string region,
            string loadBalancerArn,
            CancellationToken cancellationToken)
    {
        var response =
            await client.DescribeLoadBalancerAttributesAsync(
                new DescribeLoadBalancerAttributesRequest
                {
                    LoadBalancerArn = loadBalancerArn
                },
                cancellationToken);

        return (response.Attributes ?? [])
            .Where(attribute =>
                !string.IsNullOrWhiteSpace(attribute.Key))
            .Select(attribute =>
                new AwsLoadBalancerAttributeInfo
                {
                    LoadBalancerArn = loadBalancerArn,
                    Key = attribute.Key ?? "",
                    Value = attribute.Value ?? "",
                    Region = region
                })
            .ToList();
    }
}
