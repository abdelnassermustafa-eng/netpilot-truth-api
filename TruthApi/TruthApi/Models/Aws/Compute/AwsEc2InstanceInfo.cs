namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents a live EC2 instance and its principal infrastructure
/// relationships.
/// </summary>
public sealed class AwsEc2InstanceInfo
{
    public string InstanceId { get; init; } = "";

    public string Name { get; init; } = "";

    public string ImageId { get; init; } = "";

    public string InstanceType { get; init; } = "";

    public string Architecture { get; init; } = "";

    public string Platform { get; init; } = "";

    public string PlatformDetails { get; init; } = "";

    public string State { get; init; } = "";

    public int StateCode { get; init; }

    public string StateTransitionReason { get; init; } = "";

    public DateTimeOffset? LaunchTime { get; init; }

    public string VpcId { get; init; } = "";

    public string SubnetId { get; init; } = "";

    public string PrivateIpAddress { get; init; } = "";

    public string PrivateDnsName { get; init; } = "";

    public string PublicIpAddress { get; init; } = "";

    public string PublicDnsName { get; init; } = "";

    public string KeyName { get; init; } = "";

    public string VirtualizationType { get; init; } = "";

    public string Hypervisor { get; init; } = "";

    public string RootDeviceType { get; init; } = "";

    public string RootDeviceName { get; init; } = "";

    public bool SourceDestinationCheck { get; init; }

    public bool EbsOptimized { get; init; }

    public bool EnaSupport { get; init; }

    public string ClientToken { get; init; } = "";

    public string Region { get; init; } = "";

    public AwsInstancePlacementInfo Placement { get; init; } = new();

    public AwsInstanceProfileInfo? IamInstanceProfile { get; init; }

    public IReadOnlyList<string> NetworkInterfaceIds { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<AwsInstanceSecurityGroupInfo> SecurityGroups
    { get; init; } =
        Array.Empty<AwsInstanceSecurityGroupInfo>();

    public IReadOnlyList<string> SecurityGroupIds { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<AwsInstanceBlockDeviceInfo> BlockDevices
    { get; init; } =
        Array.Empty<AwsInstanceBlockDeviceInfo>();

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
