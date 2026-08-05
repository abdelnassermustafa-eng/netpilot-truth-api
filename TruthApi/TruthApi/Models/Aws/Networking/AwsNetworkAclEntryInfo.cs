namespace TruthApi.Models.Aws.Networking;

public sealed class AwsNetworkAclEntryInfo
{
    public int RuleNumber { get; init; }

    public bool IsEgress { get; init; }

    public string Direction => IsEgress ? "Egress" : "Ingress";

    public string Protocol { get; init; } = "";

    public string RuleAction { get; init; } = "";

    public string CidrBlock { get; init; } = "";

    public string Ipv6CidrBlock { get; init; } = "";

    public int? FromPort { get; init; }

    public int? ToPort { get; init; }

    public int? IcmpType { get; init; }

    public int? IcmpCode { get; init; }
}
