namespace TruthApi.Models.Aws.Networking;

public sealed class AwsVpcInfo
{
    public string VpcId { get; init; } = "";

    public string Name { get; init; } = "";

    public string CidrBlock { get; init; } = "";

    public string State { get; init; } = "";

    public bool IsDefault { get; init; }

    public string Region { get; init; } = "";

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
