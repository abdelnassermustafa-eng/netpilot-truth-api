namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents a legacy EC2 Auto Scaling launch configuration.
/// </summary>
public sealed class AwsLaunchConfigurationInfo
{
    public string LaunchConfigurationName { get; init; } = "";

    public string LaunchConfigurationArn { get; init; } = "";

    public string ImageId { get; init; } = "";

    public string InstanceType { get; init; } = "";

    public string KeyName { get; init; } = "";

    public string IamInstanceProfile { get; init; } = "";

    public string KernelId { get; init; } = "";

    public string RamdiskId { get; init; } = "";

    public string PlacementTenancy { get; init; } = "";

    public string SpotPrice { get; init; } = "";

    public bool EbsOptimized { get; init; }

    public bool AssociatePublicIpAddress { get; init; }

    public bool DetailedMonitoringEnabled { get; init; }

    public bool HasUserData { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public string Region { get; init; } = "";

    public IReadOnlyList<string> SecurityGroupIds { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<AwsLaunchConfigurationBlockDeviceInfo>
        BlockDevices
    { get; init; } =
        Array.Empty<AwsLaunchConfigurationBlockDeviceInfo>();
}
