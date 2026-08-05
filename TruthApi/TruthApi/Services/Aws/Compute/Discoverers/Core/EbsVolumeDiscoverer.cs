using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Compute;
using TruthApi.Services.Aws.Networking.Infrastructure;

namespace TruthApi.Services.Aws.Compute.Discoverers.Core;

/// <summary>
/// Discovers all Amazon EBS volumes in one AWS Region.
/// </summary>
public sealed class EbsVolumeDiscoverer
{
    public async Task<IReadOnlyList<AwsEbsVolumeInfo>> DiscoverAsync(
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

        var results = new List<AwsEbsVolumeInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeVolumesAsync(
                new DescribeVolumesRequest
                {
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var volume in response.Volumes ?? [])
            {
                var tags = AwsTagHelper.ToDictionary(volume.Tags);

                results.Add(new AwsEbsVolumeInfo
                {
                    VolumeId = volume.VolumeId ?? "",
                    Name = AwsTagHelper.GetName(tags),
                    VolumeType = volume.VolumeType?.Value ?? "",
                    State = volume.State?.Value ?? "",
                    SizeGiB = volume.Size ?? 0,
                    Iops = volume.Iops,
                    ThroughputMiBps = volume.Throughput,
                    Encrypted = volume.Encrypted ?? false,
                    KmsKeyId = volume.KmsKeyId ?? "",
                    SnapshotId = volume.SnapshotId ?? "",
                    MultiAttachEnabled =
                        volume.MultiAttachEnabled ?? false,
                    FastRestored = volume.FastRestored ?? false,
                    AvailabilityZone =
                        volume.AvailabilityZone ?? "",
                    AvailabilityZoneId =
                        volume.AvailabilityZoneId ?? "",
                    CreatedAt = volume.CreateTime,
                    Region = region,
                    Attachments = (volume.Attachments ?? [])
                        .Select(ToAttachmentInfo)
                        .ToList(),
                    Tags = tags
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }

    private static AwsEbsVolumeAttachmentInfo ToAttachmentInfo(
        VolumeAttachment attachment)
    {
        return new AwsEbsVolumeAttachmentInfo
        {
            VolumeId = attachment.VolumeId ?? "",
            InstanceId = attachment.InstanceId ?? "",
            Device = attachment.Device ?? "",
            State = attachment.State?.Value ?? "",
            DeleteOnTermination =
                attachment.DeleteOnTermination ?? false,
            AttachTime = attachment.AttachTime,
            AssociatedResource =
                attachment.AssociatedResource ?? "",
            InstanceOwningService =
                attachment.InstanceOwningService ?? ""
        };
    }
}
