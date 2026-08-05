using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
using TruthApi.Models.Aws.Compute;

namespace TruthApi.Services.Aws.Compute.Discoverers.Scaling;

/// <summary>
/// Discovers recent EC2 Auto Scaling activities in one AWS Region.
/// </summary>
public sealed class AutoScalingActivityDiscoverer
{
    public async Task<IReadOnlyList<AwsAutoScalingActivityInfo>>
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

        var results = new List<AwsAutoScalingActivityInfo>();
        string? nextToken = null;

        do
        {
            var response =
                await client.DescribeScalingActivitiesAsync(
                    new DescribeScalingActivitiesRequest
                    {
                        MaxRecords = 100,
                        NextToken = nextToken
                    },
                    cancellationToken);

            foreach (var activity in response.Activities ?? [])
            {
                results.Add(new AwsAutoScalingActivityInfo
                {
                    ActivityId = activity.ActivityId ?? "",
                    AutoScalingGroupName =
                        activity.AutoScalingGroupName ?? "",
                    AutoScalingGroupArn =
                        activity.AutoScalingGroupARN ?? "",
                    AutoScalingGroupState =
                        activity.AutoScalingGroupState ?? "",
                    Description = activity.Description ?? "",
                    Cause = activity.Cause ?? "",
                    Details = activity.Details ?? "",
                    StatusCode =
                        activity.StatusCode?.Value ?? "",
                    StatusMessage =
                        activity.StatusMessage ?? "",
                    ProgressPercent = activity.Progress ?? 0,
                    StartedAt = activity.StartTime,
                    EndedAt = activity.EndTime,
                    Region = region
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results
            .OrderByDescending(activity => activity.StartedAt)
            .ThenBy(activity => activity.AutoScalingGroupName)
            .ThenBy(activity => activity.ActivityId)
            .ToList();
    }
}
