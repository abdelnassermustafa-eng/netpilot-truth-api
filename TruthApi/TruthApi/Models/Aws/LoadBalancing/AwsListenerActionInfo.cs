namespace TruthApi.Models.Aws.LoadBalancing;

/// <summary>
/// Represents a default action configured on an ELBv2 listener.
/// </summary>
public sealed class AwsListenerActionInfo
{
    public int? Order { get; init; }

    public string Type { get; init; } = "";

    public string TargetGroupArn { get; init; } = "";

    public IReadOnlyList<AwsForwardTargetGroupInfo>
        ForwardTargetGroups
    { get; init; } =
        Array.Empty<AwsForwardTargetGroupInfo>();

    public bool ForwardStickinessEnabled { get; init; }

    public int? ForwardStickinessDurationSeconds { get; init; }

    public string RedirectProtocol { get; init; } = "";

    public string RedirectHost { get; init; } = "";

    public string RedirectPort { get; init; } = "";

    public string RedirectPath { get; init; } = "";

    public string RedirectQuery { get; init; } = "";

    public string RedirectStatusCode { get; init; } = "";

    public string FixedResponseContentType { get; init; } = "";

    public string FixedResponseMessageBody { get; init; } = "";

    public string FixedResponseStatusCode { get; init; } = "";

    public string AuthenticateOidcIssuer { get; init; } = "";

    public string AuthenticateOidcClientId { get; init; } = "";

    public string AuthenticateCognitoUserPoolArn { get; init; } = "";

    public string AuthenticateCognitoUserPoolClientId { get; init; } = "";

    public string AuthenticateCognitoUserPoolDomain { get; init; } = "";
}
