using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using TruthApi.Models.Aws.LoadBalancing;

namespace TruthApi.Services.Aws.LoadBalancing.Discoverers;

/// <summary>
/// Discovers ELBv2 target groups, attributes, and tags in one Region.
/// </summary>
public sealed class TargetGroupDiscoverer
{
    public async Task<IReadOnlyList<AwsTargetGroupInfo>> DiscoverAsync(
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

        var targetGroups = await GetAllTargetGroupsAsync(
            client,
            cancellationToken);

        if (targetGroups.Count == 0)
        {
            return Array.Empty<AwsTargetGroupInfo>();
        }

        var arns = targetGroups
            .Select(group => group.TargetGroupArn)
            .Where(arn => !string.IsNullOrWhiteSpace(arn))
            .Select(arn => arn!)
            .ToList();

        var tagsTask = GetTagsByArnAsync(
            client,
            arns,
            cancellationToken);

        var attributeTasks = arns
            .ToDictionary(
                arn => arn,
                arn => GetAttributesAsync(
                    client,
                    arn,
                    cancellationToken),
                StringComparer.OrdinalIgnoreCase);

        var discoveryTasks = attributeTasks.Values
            .Cast<Task>()
            .ToList();

        discoveryTasks.Add(tagsTask);

        await Task.WhenAll(discoveryTasks);

        var tagsByArn = await tagsTask;

        return targetGroups
            .Select(group =>
            {
                var arn = group.TargetGroupArn ?? "";

                tagsByArn.TryGetValue(arn, out var tags);

                IReadOnlyDictionary<string, string> attributes =
                    new Dictionary<string, string>(
                        StringComparer.OrdinalIgnoreCase);

                if (attributeTasks.TryGetValue(
                        arn,
                        out var attributeTask))
                {
                    attributes = attributeTask.Result;
                }

                return new AwsTargetGroupInfo
                {
                    TargetGroupArn = arn,
                    Name = group.TargetGroupName ?? "",
                    Protocol = group.Protocol?.Value ?? "",
                    ProtocolVersion = group.ProtocolVersion ?? "",
                    Port = group.Port,
                    TargetType = group.TargetType?.Value ?? "",
                    IpAddressType =
                        group.IpAddressType?.Value ?? "",
                    VpcId = group.VpcId ?? "",

                    HealthCheckEnabled =
                        group.HealthCheckEnabled ?? false,
                    HealthCheckProtocol =
                        group.HealthCheckProtocol?.Value ?? "",
                    HealthCheckPortExpression =
                        group.HealthCheckPort ?? "",
                    HealthCheckPort =
                        ParsePort(group.HealthCheckPort),
                    HealthCheckPath =
                        group.HealthCheckPath ?? "",
                    HealthCheckIntervalSeconds =
                        group.HealthCheckIntervalSeconds,
                    HealthCheckTimeoutSeconds =
                        group.HealthCheckTimeoutSeconds,
                    HealthyThresholdCount =
                        group.HealthyThresholdCount,
                    UnhealthyThresholdCount =
                        group.UnhealthyThresholdCount,

                    MatcherHttpCode =
                        group.Matcher?.HttpCode ?? "",
                    MatcherGrpcCode =
                        group.Matcher?.GrpcCode ?? "",

                    Region = region,

                    LoadBalancerArns =
                        (group.LoadBalancerArns ?? [])
                            .Where(value =>
                                !string.IsNullOrWhiteSpace(value))
                            .OrderBy(value => value)
                            .ToList(),

                    Attributes = attributes,

                    Tags = tags ??
                        new Dictionary<string, string>(
                            StringComparer.OrdinalIgnoreCase)
                };
            })
            .OrderBy(group => group.Name)
            .ThenBy(group => group.TargetGroupArn)
            .ToList();
    }

    private static async Task<IReadOnlyList<TargetGroup>>
        GetAllTargetGroupsAsync(
            IAmazonElasticLoadBalancingV2 client,
            CancellationToken cancellationToken)
    {
        var results = new List<TargetGroup>();
        string? marker = null;

        do
        {
            var response = await client.DescribeTargetGroupsAsync(
                new DescribeTargetGroupsRequest
                {
                    Marker = marker,
                    PageSize = 100
                },
                cancellationToken);

            results.AddRange(response.TargetGroups ?? []);
            marker = response.NextMarker;
        }
        while (!string.IsNullOrWhiteSpace(marker));

        return results;
    }

    private static async Task<
        IReadOnlyDictionary<string, string>>
        GetAttributesAsync(
            IAmazonElasticLoadBalancingV2 client,
            string targetGroupArn,
            CancellationToken cancellationToken)
    {
        var response =
            await client.DescribeTargetGroupAttributesAsync(
                new DescribeTargetGroupAttributesRequest
                {
                    TargetGroupArn = targetGroupArn
                },
                cancellationToken);

        return (response.Attributes ?? [])
            .Where(attribute =>
                !string.IsNullOrWhiteSpace(attribute.Key))
            .GroupBy(
                attribute => attribute.Key,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Last().Value ?? "",
                StringComparer.OrdinalIgnoreCase);
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

    private static int? ParsePort(string? value)
    {
        return int.TryParse(value, out var port)
            ? port
            : null;
    }
}
