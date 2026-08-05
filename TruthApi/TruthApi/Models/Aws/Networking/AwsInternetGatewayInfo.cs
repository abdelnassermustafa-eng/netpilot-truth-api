namespace TruthApi.Models.Aws.Networking;

public sealed class AwsInternetGatewayInfo
{
    public string InternetGatewayId { get; init; } = "";

    public string Name { get; init; } = "";

    public string Region { get; init; } = "";

    public IReadOnlyList<AwsInternetGatewayAttachmentInfo> Attachments
    { get; init; } =
        Array.Empty<AwsInternetGatewayAttachmentInfo>();

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
