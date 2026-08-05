namespace TruthApi.Models.Aws.Networking;

public sealed class AwsNetworkAclAssociationInfo
{
    public string AssociationId { get; init; } = "";

    public string NetworkAclId { get; init; } = "";

    public string SubnetId { get; init; } = "";
}
