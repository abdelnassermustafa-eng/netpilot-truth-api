namespace TruthApi.Models.Aws.LoadBalancing;

public sealed class AwsListenerMutualAuthenticationInfo
{
    public string Mode { get; init; } = "";

    public string TrustStoreArn { get; init; } = "";

    public bool IgnoreClientCertificateExpiry { get; init; }

    public string TrustStoreAssociationStatus { get; init; } = "";

    public string AdvertiseTrustStoreCaNames { get; init; } = "";
}
