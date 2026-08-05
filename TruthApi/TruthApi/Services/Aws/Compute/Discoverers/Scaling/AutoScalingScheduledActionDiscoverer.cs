using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
using TruthApi.Models.Aws.Compute;

namespace TruthApi.Services.Aws.Compute.Discoverers.Scaling;

/// <summary>
/// Discovers pending and recurring EC2 Auto Scaling scheduled actions
/// in one AWS Region.
/// </summary>
public sealed class AutoScalingScheduledActionDiscoverer
{
    public async Task<
        IReadOnlyList<AwsAutoScalingScheduledActionInfo>>
        DiscoverAsync(
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

        var results =
            new List<AwsAutoScalingScheduledActionInfo>();

        string? nextToken = null;

        do
        {
            var response =
                await client.DescribeScheduledActionsAsync(
                    new DescribeScheduledActionsRequest
                    {
                        MaxRecords = 100,
                        NextToken = nextToken
                    },
                    cancellationToken);

            foreach (var action in
                     response.ScheduledUpdateGroupActions ?? [])
            {
                results.Add(
                    new AwsAutoScalingScheduledActionInfo
                    {
                        ScheduledActionName =
                            action.ScheduledActionName ?? "",
                        ScheduledActionArn =
                            action.ScheduledActionARN ?? "",
                        AutoScalingGroupName =
                            action.AutoScalingGroupName ?? "",
                        MinSize = action.MinSize,
                        MaxSize = action.MaxSize,
                        DesiredCapacity =
                            action.DesiredCapacity,
                        StartTime = action.StartTime,
                        EndTime = action.EndTime,
                        LegacyTime = action.Time,
                        Recurrence = action.Recurrence ?? "",
                        TimeZone = action.TimeZone ?? "",
                        Region = region
                    });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results
            .OrderBy(action => action.AutoScalingGroupName)
            .ThenBy(action => action.ScheduledActionName)
            .ToList();
    }
}
