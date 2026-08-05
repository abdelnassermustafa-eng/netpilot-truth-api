using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
using TruthApi.Models.Aws.Compute;
using AsgInstance = Amazon.AutoScaling.Model.Instance;

namespace TruthApi.Services.Aws.Compute.Discoverers.Scaling;

/// <summary>
/// Discovers warm-pool configuration and instances for known
/// Auto Scaling groups in one AWS Region.
/// </summary>
public sealed class AutoScalingWarmPoolDiscoverer
{
    public async Task<IReadOnlyList<AwsAutoScalingWarmPoolInfo>>
        DiscoverAsync(
            IAmazonAutoScaling client,
            string region,
            IReadOnlyCollection<string> autoScalingGroupNames,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(autoScalingGroupNames);

        if (string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException(
                "AWS Region is required.",
                nameof(region));
        }

        var groupNames = autoScalingGroupNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name)
            .ToList();

        if (groupNames.Count == 0)
        {
            return Array.Empty<AwsAutoScalingWarmPoolInfo>();
        }

        var tasks = groupNames
            .Select(groupName =>
                DiscoverGroupWarmPoolAsync(
                    client,
                    region,
                    groupName,
                    cancellationToken))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        return results
            .Where(result => result is not null)
            .Select(result => result!)
            .OrderBy(result => result.AutoScalingGroupName)
            .ToList();
    }

    private static async Task<AwsAutoScalingWarmPoolInfo?>
        DiscoverGroupWarmPoolAsync(
            IAmazonAutoScaling client,
            string region,
            string autoScalingGroupName,
            CancellationToken cancellationToken)
    {
        var instances = new List<AwsAutoScalingWarmPoolInstanceInfo>();
        WarmPoolConfiguration? configuration = null;
        string? nextToken = null;

        do
        {
            var response = await client.DescribeWarmPoolAsync(
                new DescribeWarmPoolRequest
                {
                    AutoScalingGroupName = autoScalingGroupName,
                    MaxRecords = 50,
                    NextToken = nextToken
                },
                cancellationToken);

            configuration ??= response.WarmPoolConfiguration;

            instances.AddRange(
                (response.Instances ?? [])
                    .Select(ToInstanceInfo));

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        if (configuration is null && instances.Count == 0)
        {
            return null;
        }

        return new AwsAutoScalingWarmPoolInfo
        {
            AutoScalingGroupName = autoScalingGroupName,
            MinSize = configuration?.MinSize,
            MaxGroupPreparedCapacity =
                configuration?.MaxGroupPreparedCapacity,
            PoolState = configuration?.PoolState?.Value ?? "",
            Status = configuration?.Status?.Value ?? "",
            ReuseOnScaleIn =
                configuration?
                    .InstanceReusePolicy?
                    .ReuseOnScaleIn ?? false,
            Region = region,
            Instances = instances
        };
    }

    private static AwsAutoScalingWarmPoolInstanceInfo ToInstanceInfo(
        AsgInstance instance)
    {
        return new AwsAutoScalingWarmPoolInstanceInfo
        {
            InstanceId = instance.InstanceId ?? "",
            InstanceType = instance.InstanceType ?? "",
            ImageId = instance.ImageId ?? "",
            AvailabilityZone = instance.AvailabilityZone ?? "",
            HealthStatus = instance.HealthStatus ?? "",
            LifecycleState =
                instance.LifecycleState?.Value ?? "",
            ProtectedFromScaleIn =
                instance.ProtectedFromScaleIn ?? false,
            WeightedCapacity =
                instance.WeightedCapacity ?? "",
            LaunchConfigurationName =
                instance.LaunchConfigurationName ?? "",
            LaunchTemplateId =
                instance.LaunchTemplate?.LaunchTemplateId ?? "",
            LaunchTemplateName =
                instance.LaunchTemplate?.LaunchTemplateName ?? "",
            LaunchTemplateVersion =
                instance.LaunchTemplate?.Version ?? ""
        };
    }
}
