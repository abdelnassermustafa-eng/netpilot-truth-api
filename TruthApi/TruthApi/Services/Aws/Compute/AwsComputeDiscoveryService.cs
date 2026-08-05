using TruthApi.Models.Aws.Compute;
using TruthApi.Services.Aws.Compute.Discoverers.Core;
using TruthApi.Services.Aws.Compute.Discoverers.Scaling;

namespace TruthApi.Services.Aws.Compute;

/// <summary>
/// Coordinates live AWS compute discovery across one or more Regions.
/// </summary>
public sealed class AwsComputeDiscoveryService
{
    private readonly AwsClientFactory _clientFactory;
    private readonly AwsIdentityService _identityService;
    private readonly Ec2InstanceDiscoverer _instanceDiscoverer;
    private readonly EbsVolumeDiscoverer _volumeDiscoverer;
    private readonly EbsSnapshotDiscoverer _snapshotDiscoverer;
    private readonly AmiDiscoverer _amiDiscoverer;
    private readonly Ec2KeyPairDiscoverer _keyPairDiscoverer;
    private readonly LaunchTemplateDiscoverer _launchTemplateDiscoverer;
    private readonly AutoScalingGroupDiscoverer _autoScalingGroupDiscoverer;

    public AwsComputeDiscoveryService(
        AwsClientFactory clientFactory,
        AwsIdentityService identityService,
        Ec2InstanceDiscoverer instanceDiscoverer,
        EbsVolumeDiscoverer volumeDiscoverer,
        EbsSnapshotDiscoverer snapshotDiscoverer,
        AmiDiscoverer amiDiscoverer,
        Ec2KeyPairDiscoverer keyPairDiscoverer,
        LaunchTemplateDiscoverer launchTemplateDiscoverer,
        AutoScalingGroupDiscoverer autoScalingGroupDiscoverer)
    {
        _clientFactory = clientFactory;
        _identityService = identityService;
        _instanceDiscoverer = instanceDiscoverer;
        _volumeDiscoverer = volumeDiscoverer;
        _snapshotDiscoverer = snapshotDiscoverer;
        _amiDiscoverer = amiDiscoverer;
        _keyPairDiscoverer = keyPairDiscoverer;
        _launchTemplateDiscoverer = launchTemplateDiscoverer;
        _autoScalingGroupDiscoverer = autoScalingGroupDiscoverer;
    }

    public async Task<AwsComputeInventory> DiscoverAsync(
        IReadOnlyCollection<string>? requestedRegions = null,
        CancellationToken cancellationToken = default)
    {
        var identity = await _identityService.GetCurrentIdentityAsync(
            cancellationToken);

        var regions = NormalizeRegions(requestedRegions);

        var regionalTasks = regions
            .Select(region =>
                DiscoverRegionAsync(region, cancellationToken))
            .ToArray();

        var regionalResults = await Task.WhenAll(regionalTasks);

        return new AwsComputeInventory
        {
            AccountId = identity.AccountId,
            Regions = regions,
            Instances = regionalResults
                .SelectMany(result => result.Instances)
                .OrderBy(instance => instance.Region)
                .ThenBy(instance => instance.Name)
                .ThenBy(instance => instance.InstanceId)
                .ToList(),
            Volumes = regionalResults
                .SelectMany(result => result.Volumes)
                .OrderBy(volume => volume.Region)
                .ThenBy(volume => volume.Name)
                .ThenBy(volume => volume.VolumeId)
                .ToList(),
            Snapshots = regionalResults
                .SelectMany(result => result.Snapshots)
                .OrderBy(snapshot => snapshot.Region)
                .ThenBy(snapshot => snapshot.Name)
                .ThenBy(snapshot => snapshot.SnapshotId)
                .ToList(),
            Images = regionalResults
                .SelectMany(result => result.Images)
                .OrderBy(image => image.Region)
                .ThenBy(image => image.Name)
                .ThenBy(image => image.ImageId)
                .ToList(),
            KeyPairs = regionalResults
                .SelectMany(result => result.KeyPairs)
                .OrderBy(keyPair => keyPair.Region)
                .ThenBy(keyPair => keyPair.KeyName)
                .ThenBy(keyPair => keyPair.KeyPairId)
                .ToList(),
            LaunchTemplates = regionalResults
                .SelectMany(result => result.LaunchTemplates)
                .OrderBy(template => template.Region)
                .ThenBy(template => template.LaunchTemplateName)
                .ThenBy(template => template.LaunchTemplateId)
                .ToList(),
            AutoScalingGroups = regionalResults
                .SelectMany(result => result.AutoScalingGroups)
                .OrderBy(group => group.Region)
                .ThenBy(group => group.AutoScalingGroupName)
                .ToList(),
            Warnings = regionalResults
                .SelectMany(result => result.Warnings)
                .ToList(),
            DiscoveredAt = DateTimeOffset.UtcNow
        };
    }

    private IReadOnlyList<string> NormalizeRegions(
        IReadOnlyCollection<string>? requestedRegions)
    {
        var regions = requestedRegions?
            .Where(region => !string.IsNullOrWhiteSpace(region))
            .Select(region => region.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(region => region)
            .ToList();

        if (regions is { Count: > 0 })
        {
            return regions;
        }

        return new[] { _clientFactory.DefaultRegion };
    }

    private async Task<RegionalComputeResult> DiscoverRegionAsync(
        string region,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _clientFactory.GetEc2Client(region);
            var autoScalingClient = _clientFactory.GetAutoScalingClient(region);

            var instancesTask =
                _instanceDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var volumesTask =
                _volumeDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var snapshotsTask =
                _snapshotDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var imagesTask =
                _amiDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var keyPairsTask =
                _keyPairDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var launchTemplatesTask =
                _launchTemplateDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var autoScalingGroupsTask =
                _autoScalingGroupDiscoverer.DiscoverAsync(
                    autoScalingClient,
                    region,
                    cancellationToken);

            await Task.WhenAll(
                instancesTask,
                volumesTask,
                snapshotsTask,
                imagesTask,
                keyPairsTask,
                launchTemplatesTask,
                autoScalingGroupsTask);

            return new RegionalComputeResult
            {
                Instances = await instancesTask,
                Volumes = await volumesTask,
                Snapshots = await snapshotsTask,
                Images = await imagesTask,
                KeyPairs = await keyPairsTask,
                LaunchTemplates = await launchTemplatesTask,
                AutoScalingGroups = await autoScalingGroupsTask
            };
        }
        catch (Exception exception)
        {
            return new RegionalComputeResult
            {
                Warnings =
                [
                    $"Region {region} could not be discovered: " +
                    exception.Message
                ]
            };
        }
    }

    private sealed class RegionalComputeResult
    {
        public IReadOnlyList<AwsEc2InstanceInfo> Instances { get; init; } =
            Array.Empty<AwsEc2InstanceInfo>();

        public IReadOnlyList<AwsEbsVolumeInfo> Volumes { get; init; } =
            Array.Empty<AwsEbsVolumeInfo>();

        public IReadOnlyList<AwsEbsSnapshotInfo> Snapshots { get; init; } =
            Array.Empty<AwsEbsSnapshotInfo>();

        public IReadOnlyList<AwsAmiInfo> Images { get; init; } =
            Array.Empty<AwsAmiInfo>();

        public IReadOnlyList<AwsEc2KeyPairInfo> KeyPairs
        { get; init; } =
            Array.Empty<AwsEc2KeyPairInfo>();

        public IReadOnlyList<AwsLaunchTemplateInfo> LaunchTemplates
        { get; init; } =
            Array.Empty<AwsLaunchTemplateInfo>();

        public IReadOnlyList<AwsAutoScalingGroupInfo> AutoScalingGroups
        { get; init; } =
            Array.Empty<AwsAutoScalingGroupInfo>();

        public IReadOnlyList<string> Warnings { get; init; } =
            Array.Empty<string>();
    }
}
