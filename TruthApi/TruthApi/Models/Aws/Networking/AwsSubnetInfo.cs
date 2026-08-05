namespace TruthApi.Models.Aws.Networking;

public sealed class AwsSubnetInfo
{
    public string SubnetId { get; init; } = "";

    public string Name { get; init; } = "";

    public string VpcId { get; init; } = "";

    public string CidrBlock { get; init; } = "";

    public string AvailabilityZone { get; init; } = "";

    public string AvailabilityZoneId { get; init; } = "";

    public string State { get; init; } = "";

    public bool MapPublicIpOnLaunch { get; init; }

    public int AvailableIpAddressCount { get; init; }

    public string Region { get; init; } = "";

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
