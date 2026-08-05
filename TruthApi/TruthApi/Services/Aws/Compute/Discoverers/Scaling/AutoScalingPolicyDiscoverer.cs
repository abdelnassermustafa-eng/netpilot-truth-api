using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
using TruthApi.Models.Aws.Compute;

namespace TruthApi.Services.Aws.Compute.Discoverers.Scaling;

/// <summary>
/// Discovers Auto Scaling policies in one AWS Region.
/// </summary>
public sealed class AutoScalingPolicyDiscoverer
{
    public async Task<IReadOnlyList<AwsAutoScalingPolicyInfo>>
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

        var results = new List<AwsAutoScalingPolicyInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribePoliciesAsync(
                new DescribePoliciesRequest
                {
                    MaxRecords = 100,
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var policy in response.ScalingPolicies ?? [])
            {
                results.Add(new AwsAutoScalingPolicyInfo
                {
                    PolicyName = policy.PolicyName ?? "",
                    PolicyArn = policy.PolicyARN ?? "",
                    AutoScalingGroupName =
                        policy.AutoScalingGroupName ?? "",
                    PolicyType = policy.PolicyType ?? "",
                    Enabled = policy.Enabled ?? false,
                    AdjustmentType = policy.AdjustmentType ?? "",
                    ScalingAdjustment = policy.ScalingAdjustment,
                    MinAdjustmentMagnitude =
                        policy.MinAdjustmentMagnitude,
                    MinAdjustmentStep =
                        policy.MinAdjustmentStep,
                    CooldownSeconds = policy.Cooldown,
                    EstimatedInstanceWarmupSeconds =
                        policy.EstimatedInstanceWarmup,
                    MetricAggregationType =
                        policy.MetricAggregationType ?? "",
                    HasPredictiveScalingConfiguration =
                        policy.PredictiveScalingConfiguration is not null,
                    Region = region,
                    TargetTracking = ToTargetTrackingInfo(
                        policy.TargetTrackingConfiguration),
                    StepAdjustments =
                        (policy.StepAdjustments ?? [])
                            .Select(ToStepAdjustmentInfo)
                            .ToList(),
                    Alarms =
                        (policy.Alarms ?? [])
                            .Select(ToAlarmInfo)
                            .ToList()
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results
            .OrderBy(policy => policy.AutoScalingGroupName)
            .ThenBy(policy => policy.PolicyName)
            .ToList();
    }

    private static AwsScalingPolicyAlarmInfo ToAlarmInfo(
        Alarm alarm)
    {
        return new AwsScalingPolicyAlarmInfo
        {
            AlarmName = alarm.AlarmName ?? "",
            AlarmArn = alarm.AlarmARN ?? ""
        };
    }

    private static AwsScalingPolicyStepAdjustmentInfo
        ToStepAdjustmentInfo(
            StepAdjustment adjustment)
    {
        return new AwsScalingPolicyStepAdjustmentInfo
        {
            MetricIntervalLowerBound =
                adjustment.MetricIntervalLowerBound,
            MetricIntervalUpperBound =
                adjustment.MetricIntervalUpperBound,
            ScalingAdjustment =
                adjustment.ScalingAdjustment
        };
    }

    private static AwsTargetTrackingPolicyInfo? ToTargetTrackingInfo(
        TargetTrackingConfiguration? configuration)
    {
        if (configuration is null)
        {
            return null;
        }

        return new AwsTargetTrackingPolicyInfo
        {
            TargetValue = configuration.TargetValue,
            DisableScaleIn =
                configuration.DisableScaleIn ?? false,
            PredefinedMetricType =
                configuration
                    .PredefinedMetricSpecification?
                    .PredefinedMetricType?
                    .Value ?? "",
            ResourceLabel =
                configuration
                    .PredefinedMetricSpecification?
                    .ResourceLabel ?? "",
            UsesCustomizedMetric =
                configuration.CustomizedMetricSpecification is not null
        };
    }
}
