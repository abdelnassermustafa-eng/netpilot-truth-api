using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
using TruthApi.Models.Aws.Compute;

namespace TruthApi.Services.Aws.Compute.Discoverers.Scaling;

/// <summary>
/// Discovers recent instance-refresh operations for known Auto Scaling
/// groups in one AWS Region.
/// </summary>
public sealed class AutoScalingInstanceRefreshDiscoverer
{
    public async Task<
        IReadOnlyList<AwsAutoScalingInstanceRefreshInfo>>
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
            return Array.Empty<AwsAutoScalingInstanceRefreshInfo>();
        }

        var tasks = groupNames
            .Select(groupName =>
                DiscoverGroupRefreshesAsync(
                    client,
                    region,
                    groupName,
                    cancellationToken))
            .ToArray();

        var groupResults = await Task.WhenAll(tasks);

        return groupResults
            .SelectMany(result => result)
            .OrderByDescending(refresh => refresh.StartedAt)
            .ThenBy(refresh => refresh.AutoScalingGroupName)
            .ThenBy(refresh => refresh.InstanceRefreshId)
            .ToList();
    }

    private static async Task<
        IReadOnlyList<AwsAutoScalingInstanceRefreshInfo>>
        DiscoverGroupRefreshesAsync(
            IAmazonAutoScaling client,
            string region,
            string autoScalingGroupName,
            CancellationToken cancellationToken)
    {
        var results =
            new List<AwsAutoScalingInstanceRefreshInfo>();

        string? nextToken = null;

        do
        {
            var response =
                await client.DescribeInstanceRefreshesAsync(
                    new DescribeInstanceRefreshesRequest
                    {
                        AutoScalingGroupName =
                            autoScalingGroupName,
                        MaxRecords = 100,
                        NextToken = nextToken
                    },
                    cancellationToken);

            foreach (var refresh in response.InstanceRefreshes ?? [])
            {
                results.Add(
                    new AwsAutoScalingInstanceRefreshInfo
                    {
                        InstanceRefreshId =
                            refresh.InstanceRefreshId ?? "",
                        AutoScalingGroupName =
                            refresh.AutoScalingGroupName ??
                            autoScalingGroupName,
                        Status =
                            refresh.Status?.Value ?? "",
                        StatusReason =
                            refresh.StatusReason ?? "",
                        Strategy =
                            refresh.Strategy?.Value ?? "",
                        PercentageComplete =
                            refresh.PercentageComplete,
                        InstancesToUpdate =
                            refresh.InstancesToUpdate,
                        StartedAt =
                            refresh.StartTime,
                        EndedAt =
                            refresh.EndTime,
                        HasDesiredConfiguration =
                            refresh.DesiredConfiguration is not null,
                        HasProgressDetails =
                            refresh.ProgressDetails is not null,
                        Preferences =
                            ToPreferencesInfo(
                                refresh.Preferences),
                        Rollback =
                            ToRollbackInfo(
                                refresh.RollbackDetails),
                        Region = region
                    });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }

    private static AwsInstanceRefreshPreferencesInfo?
        ToPreferencesInfo(
            RefreshPreferences? preferences)
    {
        if (preferences is null)
        {
            return null;
        }

        return new AwsInstanceRefreshPreferencesInfo
        {
            AutoRollback =
                preferences.AutoRollback ?? false,
            SkipMatching =
                preferences.SkipMatching ?? false,
            MinHealthyPercentage =
                preferences.MinHealthyPercentage,
            MaxHealthyPercentage =
                preferences.MaxHealthyPercentage,
            InstanceWarmupSeconds =
                preferences.InstanceWarmup,
            CheckpointDelaySeconds =
                preferences.CheckpointDelay,
            BakeTimeSeconds =
                preferences.BakeTime,
            ScaleInProtectedInstances =
                preferences.ScaleInProtectedInstances?
                    .Value ?? "",
            StandbyInstances =
                preferences.StandbyInstances?
                    .Value ?? "",
            CheckpointPercentages =
                (preferences.CheckpointPercentages ?? [])
                    .ToList(),
            AlarmNames =
                (preferences.AlarmSpecification?.Alarms ?? [])
                    .Where(name =>
                        !string.IsNullOrWhiteSpace(name))
                    .ToList()
        };
    }

    private static AwsInstanceRefreshRollbackInfo?
        ToRollbackInfo(
            RollbackDetails? rollback)
    {
        if (rollback is null)
        {
            return null;
        }

        return new AwsInstanceRefreshRollbackInfo
        {
            InstancesToUpdate =
                rollback.InstancesToUpdateOnRollback,
            PercentageComplete =
                rollback.PercentageCompleteOnRollback,
            Reason =
                rollback.RollbackReason ?? "",
            StartedAt =
                rollback.RollbackStartTime
        };
    }
}
