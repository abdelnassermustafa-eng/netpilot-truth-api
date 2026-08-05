namespace TruthApi.Models.Aws.Compute;

public sealed class AwsInstancePlacementInfo
{
    public string AvailabilityZone { get; init; } = "";

    public string Tenancy { get; init; } = "";

    public string HostId { get; init; } = "";

    public string Affinity { get; init; } = "";

    public string GroupName { get; init; } = "";

    public int? PartitionNumber { get; init; }
}
