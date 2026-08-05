namespace TruthApi.Models.Aws.LoadBalancing;

public sealed class AwsListenerCertificateInfo
{
    public string CertificateArn { get; init; } = "";

    public bool IsDefault { get; init; }
}
