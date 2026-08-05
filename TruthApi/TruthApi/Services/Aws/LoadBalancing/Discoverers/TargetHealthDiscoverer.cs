using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using TruthApi.Models.Aws.LoadBalancing;

namespace TruthApi.Services.Aws.LoadBalancing.Discoverers;

/// <summary>
/// Discovers the live health state of registered targets for known ELBv2
/// target groups in one AWS Region.
/// </summary>
public sealed class TargetHealthDiscoverer
{
    public async Task<IReadOnlyList<AwsTargetHealthInfo>> DiscoverAsync(
        IAmazonElasticLoadBalancingV2 client,
        string region,
        IReadOnlyCollection<string> targetGroupArns,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(targetGroupArns);

        if (string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException(
                "AWS Region is required.",
                nameof(region));
        }

        var arns = targetGroupArns
            .Where(arn => !string.IsNullOrWhiteSpace(arn))
            .Select(arn => arn.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(arn => arn)
            .ToList();

        if (arns.Count == 0)
        {
            return Array.Empty<AwsTargetHealthInfo>();
        }

        var tasks = arns
            .Select(arn =>
                DiscoverForTargetGroupAsync(
                    client,
                    region,
                    arn,
                    cancellationToken))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        return results
            .SelectMany(result => result)
            .OrderBy(item => item.TargetGroupArn)
            .ThenBy(item => item.TargetId)
            .ThenBy(item => item.Port)
            .ToList();
    }

    private static async Task<IReadOnlyList<AwsTargetHealthInfo>>
        DiscoverForTargetGroupAsync(
            IAmazonElasticLoadBalancingV2 client,
            string region,
            string targetGroupArn,
            CancellationToken cancellationToken)
    {
        var response = await client.DescribeTargetHealthAsync(
            new DescribeTargetHealthRequest
            {
                TargetGroupArn = targetGroupArn,
                Include = ["AnomalyDetection"]
            },
            cancellationToken);

        return (response.TargetHealthDescriptions ?? [])
            .Select(description =>
                new AwsTargetHealthInfo
                {
                    TargetGroupArn = targetGroupArn,

                    TargetId =
                        description.Target?.Id ?? "",

                    Port =
                        description.Target?.Port,

                    AvailabilityZone =
                        description.Target?.AvailabilityZone ?? "",

                    HealthState =
                        description.TargetHealth?
                            .State?
                            .Value ?? "",

                    HealthReason =
                        description.TargetHealth?
                            .Reason?
                            .Value ?? "",

                    HealthDescription =
                        description.TargetHealth?
                            .Description ?? "",

                    AdministrativeOverrideState =
                        description.AdministrativeOverride?
                            .State?
                            .Value ?? "",

                    AdministrativeOverrideReason =
                        description.AdministrativeOverride?
                            .Reason?
                            .Value ?? "",

                    AdministrativeOverrideDescription =
                        description.AdministrativeOverride?
                            .Description ?? "",

                    AnomalyResult =
                        description.AnomalyDetection?
                            .Result?
                            .Value ?? "",

                    AnomalyMitigationInEffect =
                        description.AnomalyDetection?
                            .MitigationInEffect?
                            .Value ?? "",

                    Region = region
                })
            .ToList();
    }
}
