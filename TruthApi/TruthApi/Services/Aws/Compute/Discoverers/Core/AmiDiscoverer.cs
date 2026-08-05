using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Compute;
using TruthApi.Services.Aws.Networking.Infrastructure;

namespace TruthApi.Services.Aws.Compute.Discoverers.Core;

/// <summary>
/// Discovers AMIs owned by the current AWS account in one AWS Region.
/// </summary>
public sealed class AmiDiscoverer
{
    public async Task<IReadOnlyList<AwsAmiInfo>> DiscoverAsync(
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

        var results = new List<AwsAmiInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeImagesAsync(
                new DescribeImagesRequest
                {
                    Owners = ["self"],
                    IncludeDeprecated = true,
                    IncludeDisabled = true,
                    MaxResults = 100,
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var image in response.Images ?? [])
            {
                var tags = AwsTagHelper.ToDictionary(image.Tags);

                results.Add(new AwsAmiInfo
                {
                    ImageId = image.ImageId ?? "",
                    Name = image.Name ?? AwsTagHelper.GetName(tags),
                    Description = image.Description ?? "",
                    OwnerId = image.OwnerId ?? "",
                    OwnerAlias = image.ImageOwnerAlias ?? "",
                    State = image.State?.Value ?? "",
                    ImageType = image.ImageType?.Value ?? "",
                    Architecture = image.Architecture?.Value ?? "",
                    Platform = image.Platform?.Value ?? "",
                    PlatformDetails = image.PlatformDetails ?? "",
                    VirtualizationType =
                        image.VirtualizationType?.Value ?? "",
                    Hypervisor = image.Hypervisor?.Value ?? "",
                    RootDeviceType =
                        image.RootDeviceType?.Value ?? "",
                    RootDeviceName = image.RootDeviceName ?? "",
                    BootMode = image.BootMode?.Value ?? "",
                    ImdsSupport = image.ImdsSupport?.Value ?? "",
                    TpmSupport = image.TpmSupport?.Value ?? "",
                    SourceImageId = image.SourceImageId ?? "",
                    SourceImageRegion = image.SourceImageRegion ?? "",
                    SourceInstanceId = image.SourceInstanceId ?? "",
                    IsPublic = image.Public ?? false,
                    EnaSupport = image.EnaSupport ?? false,
                    IsAllowed = image.ImageAllowed,
                    DeregistrationProtection =
                        image.DeregistrationProtection ?? "",
                    CreatedAt = ParseTimestamp(image.CreationDate),
                    DeprecationTime =
                        ParseTimestamp(image.DeprecationTime),
                    LastLaunchedTime =
                        ParseTimestamp(image.LastLaunchedTime),
                    Region = region,
                    BlockDevices =
                        (image.BlockDeviceMappings ?? [])
                            .Select(ToBlockDeviceInfo)
                            .ToList(),
                    Tags = tags
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }

    private static AwsAmiBlockDeviceInfo ToBlockDeviceInfo(
        BlockDeviceMapping mapping)
    {
        return new AwsAmiBlockDeviceInfo
        {
            DeviceName = mapping.DeviceName ?? "",
            VirtualName = mapping.VirtualName ?? "",
            NoDevice = mapping.NoDevice ?? "",
            SnapshotId = mapping.Ebs?.SnapshotId ?? "",
            VolumeSizeGiB = mapping.Ebs?.VolumeSize,
            VolumeType = mapping.Ebs?.VolumeType?.Value ?? "",
            Iops = mapping.Ebs?.Iops,
            ThroughputMiBps = mapping.Ebs?.Throughput,
            Encrypted = mapping.Ebs?.Encrypted ?? false,
            DeleteOnTermination =
                mapping.Ebs?.DeleteOnTermination ?? false
        };
    }

    private static DateTimeOffset? ParseTimestamp(string? value)
    {
        return DateTimeOffset.TryParse(value, out var parsed)
            ? parsed
            : null;
    }
}
