using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
using TruthApi.Models.Aws.Compute;
using AsgInstance = Amazon.AutoScaling.Model.Instance;

namespace TruthApi.Services.Aws.Compute.Discoverers.Scaling;

/// <summary>
/// Discovers EC2 Auto Scaling groups and current member instances
/// in one AWS Region.
/// </summary>
public sealed class AutoScalingGroupDiscoverer
{
    public async Task<IReadOnlyList<AwsAutoScalingGroupInfo>> DiscoverAsync(
        IAmazonAutoScaling client,
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

        var results = new List<AwsAutoScalingGroupInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeAutoScalingGroupsAsync(
                new DescribeAutoScalingGroupsRequest
                {
                    MaxRecords = 100,
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var group in response.AutoScalingGroups ?? [])
            {
                var tags = ToTagDictionary(group.Tags);
                var propagation = ToPropagationDictionary(group.Tags);

                results.Add(new AwsAutoScalingGroupInfo
                {
                    AutoScalingGroupName =
                        group.AutoScalingGroupName ?? "",
                    AutoScalingGroupArn =
                        group.AutoScalingGroupARN ?? "",
                    Status = group.Status ?? "",
                    MinSize = group.MinSize ?? 0,
                    MaxSize = group.MaxSize ?? 0,
                    DesiredCapacity = group.DesiredCapacity ?? 0,
                    DesiredCapacityType =
                        group.DesiredCapacityType ?? "",
                    DefaultCooldownSeconds =
                        group.DefaultCooldown ?? 0,
                    DefaultInstanceWarmupSeconds =
                        group.DefaultInstanceWarmup ?? 0,
                    HealthCheckType =
                        group.HealthCheckType ?? "",
                    HealthCheckGracePeriodSeconds =
                        group.HealthCheckGracePeriod ?? 0,
                    CapacityRebalance =
                        group.CapacityRebalance ?? false,
                    NewInstancesProtectedFromScaleIn =
                        group.NewInstancesProtectedFromScaleIn ?? false,
                    MaxInstanceLifetimeSeconds =
                        group.MaxInstanceLifetime,
                    PlacementGroup =
                        group.PlacementGroup ?? "",
                    ServiceLinkedRoleArn =
                        group.ServiceLinkedRoleARN ?? "",
                    LaunchConfigurationName =
                        group.LaunchConfigurationName ?? "",
                    LaunchTemplateId =
                        group.LaunchTemplate?.LaunchTemplateId ?? "",
                    LaunchTemplateName =
                        group.LaunchTemplate?.LaunchTemplateName ?? "",
                    LaunchTemplateVersion =
                        group.LaunchTemplate?.Version ?? "",
                    UsesMixedInstancesPolicy =
                        group.MixedInstancesPolicy is not null,
                    CreatedAt = group.CreatedTime,
                    Region = region,
                    AvailabilityZones =
                        (group.AvailabilityZones ?? [])
                            .Where(value =>
                                !string.IsNullOrWhiteSpace(value))
                            .ToList(),
                    SubnetIds = ParseSubnetIds(
                        group.VPCZoneIdentifier),
                    LoadBalancerNames =
                        (group.LoadBalancerNames ?? [])
                            .Where(value =>
                                !string.IsNullOrWhiteSpace(value))
                            .ToList(),
                    TargetGroupArns =
                        (group.TargetGroupARNs ?? [])
                            .Where(value =>
                                !string.IsNullOrWhiteSpace(value))
                            .ToList(),
                    TerminationPolicies =
                        (group.TerminationPolicies ?? [])
                            .Where(value =>
                                !string.IsNullOrWhiteSpace(value))
                            .ToList(),
                    SuspendedProcesses =
                        (group.SuspendedProcesses ?? [])
                            .Select(process => process.ProcessName ?? "")
                            .Where(value =>
                                !string.IsNullOrWhiteSpace(value))
                            .ToList(),
                    Instances =
                        (group.Instances ?? [])
                            .Select(ToInstanceInfo)
                            .ToList(),
                    Tags = tags,
                    TagPropagation = propagation
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }

    private static AwsAutoScalingInstanceInfo ToInstanceInfo(
        AsgInstance instance)
    {
        return new AwsAutoScalingInstanceInfo
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

    private static IReadOnlyList<string> ParseSubnetIds(
        string? vpcZoneIdentifier)
    {
        if (string.IsNullOrWhiteSpace(vpcZoneIdentifier))
        {
            return Array.Empty<string>();
        }

        return vpcZoneIdentifier
            .Split(
                ',',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static IReadOnlyDictionary<string, string> ToTagDictionary(
        IEnumerable<TagDescription>? tags)
    {
        if (tags is null)
        {
            return new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
        }

        return tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag.Key))
            .GroupBy(
                tag => tag.Key,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Last().Value ?? "",
                StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyDictionary<string, bool>
        ToPropagationDictionary(
            IEnumerable<TagDescription>? tags)
    {
        if (tags is null)
        {
            return new Dictionary<string, bool>(
                StringComparer.OrdinalIgnoreCase);
        }

        return tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag.Key))
            .GroupBy(
                tag => tag.Key,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Last().PropagateAtLaunch ?? false,
                StringComparer.OrdinalIgnoreCase);
    }
}
