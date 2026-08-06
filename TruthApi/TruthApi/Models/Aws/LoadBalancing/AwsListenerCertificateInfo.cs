namespace TruthApi.Models.Aws.LoadBalancing;

/// <summary>
/// Represents one certificate attached to an ELBv2 listener.
/// </summary>
public sealed class AwsListenerCertificateInfo
{
    public string ListenerArn { get; init; } = "";

    public string CertificateArn { get; init; } = "";

    public bool IsDefault { get; init; }

    public string Region { get; init; } = "";
}
