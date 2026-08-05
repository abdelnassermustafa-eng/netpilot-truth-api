namespace TruthApi.Models.Aws.Networking;

public sealed class AwsRouteInfo
{
    public string Destination { get; init; } = "";

    public string TargetType { get; init; } = "";

    public string TargetId { get; init; } = "";

    public string State { get; init; } = "";

    public string Origin { get; init; } = "";
}
