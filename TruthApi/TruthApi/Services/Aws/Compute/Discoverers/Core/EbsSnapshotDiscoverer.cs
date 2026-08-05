using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Compute;
using TruthApi.Services.Aws.Networking.Infrastructure;

namespace TruthApi.Services.Aws.Compute.Discoverers.Core;

/// <summary>
/// Discovers EBS snapshots owned by the current AWS account
/// in one AWS Region.
/// </summary>
public sealed class EbsSnapshotDiscoverer
{
    public async Task<IReadOnlyList<AwsEbsSnapshotInfo>> DiscoverAsync(
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

        var results = new List<AwsEbsSnapshotInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeSnapshotsAsync(
                new DescribeSnapshotsRequest
                {
                    OwnerIds = ["self"],
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var snapshot in response.Snapshots ?? [])
            {
                var tags = AwsTagHelper.ToDictionary(snapshot.Tags);

                results.Add(new AwsEbsSnapshotInfo
                {
                    SnapshotId = snapshot.SnapshotId ?? "",
                    Name = AwsTagHelper.GetName(tags),
                    VolumeId = snapshot.VolumeId ?? "",
                    OwnerId = snapshot.OwnerId ?? "",
                    OwnerAlias = snapshot.OwnerAlias ?? "",
                    State = snapshot.State?.Value ?? "",
                    Description = snapshot.Description ?? "",
                    VolumeSizeGiB = snapshot.VolumeSize ?? 0,
                    ProgressPercent =
                        ParseProgress(snapshot.Progress),
                    Encrypted = snapshot.Encrypted ?? false,
                    KmsKeyId = snapshot.KmsKeyId ?? "",
                    DataEncryptionKeyId =
                        snapshot.DataEncryptionKeyId ?? "",
                    StorageTier =
                        snapshot.StorageTier?.Value ?? "",
                    RestoreExpiryTimePresent =
                        snapshot.RestoreExpiryTime.HasValue,
                    RestoreExpiryTime =
                        snapshot.RestoreExpiryTime,
                    StartedAt = snapshot.StartTime,
                    Region = region,
                    Tags = tags
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }

    private static int ParseProgress(string? progress)
    {
        if (string.IsNullOrWhiteSpace(progress))
        {
            return 0;
        }

        var normalized = progress.Trim().TrimEnd('%');

        return int.TryParse(normalized, out var value)
            ? value
            : 0;
    }
}
