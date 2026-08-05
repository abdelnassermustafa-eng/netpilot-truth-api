using Amazon.AutoScaling;
using Amazon.AutoScaling.Model;
using TruthApi.Models.Aws.Compute;

namespace TruthApi.Services.Aws.Compute.Discoverers.Scaling;

/// <summary>
/// Discovers legacy Auto Scaling launch configurations in one Region.
/// </summary>
public sealed class LaunchConfigurationDiscoverer
{
    public async Task<IReadOnlyList<AwsLaunchConfigurationInfo>>
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

        var results = new List<AwsLaunchConfigurationInfo>();
        string? nextToken = null;

        do
        {
            var response =
                await client.DescribeLaunchConfigurationsAsync(
                    new DescribeLaunchConfigurationsRequest
                    {
                        MaxRecords = 100,
                        NextToken = nextToken
                    },
                    cancellationToken);

            foreach (var configuration in
                     response.LaunchConfigurations ?? [])
            {
                results.Add(new AwsLaunchConfigurationInfo
                {
                    LaunchConfigurationName =
                        configuration.LaunchConfigurationName ?? "",
                    LaunchConfigurationArn =
                        configuration.LaunchConfigurationARN ?? "",
                    ImageId = configuration.ImageId ?? "",
                    InstanceType = configuration.InstanceType ?? "",
                    KeyName = configuration.KeyName ?? "",
                    IamInstanceProfile =
                        configuration.IamInstanceProfile ?? "",
                    KernelId = configuration.KernelId ?? "",
                    RamdiskId = configuration.RamdiskId ?? "",
                    PlacementTenancy =
                        configuration.PlacementTenancy ?? "",
                    SpotPrice = configuration.SpotPrice ?? "",
                    EbsOptimized =
                        configuration.EbsOptimized ?? false,
                    AssociatePublicIpAddress =
                        configuration.AssociatePublicIpAddress ?? false,
                    DetailedMonitoringEnabled =
                        configuration.InstanceMonitoring?.Enabled ??
                        false,
                    HasUserData =
                        !string.IsNullOrWhiteSpace(
                            configuration.UserData),
                    CreatedAt = configuration.CreatedTime,
                    Region = region,
                    SecurityGroupIds =
                        (configuration.SecurityGroups ?? [])
                            .Where(value =>
                                !string.IsNullOrWhiteSpace(value))
                            .ToList(),
                    BlockDevices =
                        (configuration.BlockDeviceMappings ?? [])
                            .Select(ToBlockDeviceInfo)
                            .ToList()
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results
            .OrderBy(item => item.LaunchConfigurationName)
            .ToList();
    }

    private static AwsLaunchConfigurationBlockDeviceInfo
        ToBlockDeviceInfo(
            BlockDeviceMapping mapping)
    {
        return new AwsLaunchConfigurationBlockDeviceInfo
        {
            DeviceName = mapping.DeviceName ?? "",
            VirtualName = mapping.VirtualName ?? "",
            NoDevice = mapping.NoDevice ?? false,
            SnapshotId = mapping.Ebs?.SnapshotId ?? "",
            VolumeSizeGiB = mapping.Ebs?.VolumeSize,
            VolumeType = mapping.Ebs?.VolumeType ?? "",
            Iops = mapping.Ebs?.Iops,
            ThroughputMiBps = mapping.Ebs?.Throughput,
            Encrypted = mapping.Ebs?.Encrypted ?? false,
            DeleteOnTermination =
                mapping.Ebs?.DeleteOnTermination ?? false
        };
    }
}
