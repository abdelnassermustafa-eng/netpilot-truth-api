namespace TruthApi.Models.Aws.Networking;

public sealed class AwsRouteTableInfo
{
    public string RouteTableId { get; init; } = "";

    public string Name { get; init; } = "";

    public string VpcId { get; init; } = "";

    public bool IsMain { get; init; }

    public string Region { get; init; } = "";

    public IReadOnlyList<AwsRouteInfo> Routes { get; init; } =
        Array.Empty<AwsRouteInfo>();

    public IReadOnlyList<AwsRouteTableAssociationInfo> Associations
    { get; init; } =
        Array.Empty<AwsRouteTableAssociationInfo>();

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
