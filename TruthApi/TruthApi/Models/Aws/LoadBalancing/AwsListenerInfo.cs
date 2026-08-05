namespace TruthApi.Models.Aws.LoadBalancing;

/// <summary>
/// Represents an ELBv2 listener and its default routing actions.
/// </summary>
public sealed class AwsListenerInfo
{
    public string ListenerArn { get; init; } = "";

    public string LoadBalancerArn { get; init; } = "";

    public string Protocol { get; init; } = "";

    public int? Port { get; init; }

    public string SslPolicy { get; init; } = "";

    public string Region { get; init; } = "";

    public IReadOnlyList<string> AlpnPolicies { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<AwsListenerCertificateInfo> Certificates
    { get; init; } =
        Array.Empty<AwsListenerCertificateInfo>();

    public IReadOnlyList<AwsListenerActionInfo> DefaultActions
    { get; init; } =
        Array.Empty<AwsListenerActionInfo>();

    public AwsListenerMutualAuthenticationInfo? MutualAuthentication
    { get; init; }
}
