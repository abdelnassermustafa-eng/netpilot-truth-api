using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
using TruthApi.Models.Aws.Compute;

namespace TruthApi.Services.Aws.Compute.Discoverers.Scaling;

/// <summary>
/// Discovers lifecycle hooks for known Auto Scaling groups in one Region.
/// </summary>
public sealed class AutoScalingLifecycleHookDiscoverer
{
    public async Task<IReadOnlyList<AwsAutoScalingLifecycleHookInfo>>
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
            return Array.Empty<AwsAutoScalingLifecycleHookInfo>();
        }

        var discoveryTasks = groupNames
            .Select(groupName =>
                DiscoverGroupHooksAsync(
                    client,
                    region,
                    groupName,
                    cancellationToken))
            .ToArray();

        var groupResults = await Task.WhenAll(discoveryTasks);

        return groupResults
            .SelectMany(result => result)
            .OrderBy(hook => hook.AutoScalingGroupName)
            .ThenBy(hook => hook.LifecycleHookName)
            .ToList();
    }

    private static async Task<
        IReadOnlyList<AwsAutoScalingLifecycleHookInfo>>
        DiscoverGroupHooksAsync(
            IAmazonAutoScaling client,
            string region,
            string autoScalingGroupName,
            CancellationToken cancellationToken)
    {
        var response = await client.DescribeLifecycleHooksAsync(
            new DescribeLifecycleHooksRequest
            {
                AutoScalingGroupName = autoScalingGroupName
            },
            cancellationToken);

        return (response.LifecycleHooks ?? [])
            .Select(hook =>
                new AwsAutoScalingLifecycleHookInfo
                {
                    LifecycleHookName =
                        hook.LifecycleHookName ?? "",
                    AutoScalingGroupName =
                        hook.AutoScalingGroupName ??
                        autoScalingGroupName,
                    LifecycleTransition =
                        hook.LifecycleTransition ?? "",
                    DefaultResult =
                        hook.DefaultResult ?? "",
                    HeartbeatTimeoutSeconds =
                        hook.HeartbeatTimeout,
                    GlobalTimeoutSeconds =
                        hook.GlobalTimeout,
                    NotificationTargetArn =
                        hook.NotificationTargetARN ?? "",
                    RoleArn =
                        hook.RoleARN ?? "",
                    NotificationMetadata =
                        hook.NotificationMetadata ?? "",
                    Region = region
                })
            .ToList();
    }
}
