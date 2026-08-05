namespace TruthApi.Models.Aws.Networking;

public sealed class AwsRouteTableAssociationInfo
{
    public string AssociationId { get; init; } = "";

    public string SubnetId { get; init; } = "";

    public string GatewayId { get; init; } = "";

    public bool IsMain { get; init; }

    public string State { get; init; } = "";
}
