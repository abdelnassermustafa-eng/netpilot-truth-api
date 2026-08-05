namespace TruthApi.Models.Aws.Compute;

/// <summary>
/// Represents an EC2 key pair registered in one AWS Region.
///
/// Only public metadata is returned by DescribeKeyPairs. Private key
/// material is never returned or stored by this discovery model.
/// </summary>
public sealed class AwsEc2KeyPairInfo
{
    public string KeyPairId { get; init; } = "";

    public string KeyName { get; init; } = "";

    public string KeyType { get; init; } = "";

    public string KeyFingerprint { get; init; } = "";

    public string PublicKey { get; init; } = "";

    public DateTimeOffset? CreatedAt { get; init; }

    public string Region { get; init; } = "";

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();
}
