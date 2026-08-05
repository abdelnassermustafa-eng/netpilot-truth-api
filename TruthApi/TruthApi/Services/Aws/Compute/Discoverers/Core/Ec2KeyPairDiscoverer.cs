using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Compute;
using TruthApi.Services.Aws.Networking.Infrastructure;

namespace TruthApi.Services.Aws.Compute.Discoverers.Core;

/// <summary>
/// Discovers EC2 key-pair metadata in one AWS Region.
/// </summary>
public sealed class Ec2KeyPairDiscoverer
{
    public async Task<IReadOnlyList<AwsEc2KeyPairInfo>> DiscoverAsync(
        IAmazonEC2 client,
        string region,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

        if (string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException(
                "AWS Region is required.",
                nameof(region));
        }

        var response = await client.DescribeKeyPairsAsync(
            new DescribeKeyPairsRequest(),
            cancellationToken);

        return (response.KeyPairs ?? [])
            .Select(keyPair =>
            {
                var tags = AwsTagHelper.ToDictionary(keyPair.Tags);

                return new AwsEc2KeyPairInfo
                {
                    KeyPairId = keyPair.KeyPairId ?? "",
                    KeyName = keyPair.KeyName ?? "",
                    KeyType = keyPair.KeyType?.Value ?? "",
                    KeyFingerprint = keyPair.KeyFingerprint ?? "",
                    PublicKey = keyPair.PublicKey ?? "",
                    CreatedAt = keyPair.CreateTime,
                    Region = region,
                    Tags = tags
                };
            })
            .OrderBy(keyPair => keyPair.KeyName)
            .ThenBy(keyPair => keyPair.KeyPairId)
            .ToList();
    }
}
