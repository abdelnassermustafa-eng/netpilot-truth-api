namespace TruthApi.Models.Aws.Networking;

public sealed class AwsNetworkAclInfo
{
    public string NetworkAclId { get; init; } = "";

    public string Name { get; init; } = "";

    public string VpcId { get; init; } = "";

    public string OwnerId { get; init; } = "";

    public bool IsDefault { get; init; }

    public string Region { get; init; } = "";

    public IReadOnlyList<AwsNetworkAclAssociationInfo> Associations
    { get; init; } =
        Array.Empty<AwsNetworkAclAssociationInfo>();

    public IReadOnlyList<AwsNetworkAclEntryInfo> Entries { get; init; } =
        Array.Empty<AwsNetworkAclEntryInfo>();

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
