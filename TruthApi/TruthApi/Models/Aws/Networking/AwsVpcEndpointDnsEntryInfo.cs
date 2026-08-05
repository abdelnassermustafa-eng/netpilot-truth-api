namespace TruthApi.Models.Aws.Networking;

public sealed class AwsVpcEndpointDnsEntryInfo
{
    public string DnsName { get; init; } = "";

    public string HostedZoneId { get; init; } = "";
}
