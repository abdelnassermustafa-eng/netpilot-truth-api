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
    private readonly LaunchConfigurationDiscoverer _launchConfigurationDiscoverer;
    private readonly AutoScalingPolicyDiscoverer _autoScalingPolicyDiscoverer;
    private readonly AutoScalingScheduledActionDiscoverer _scheduledActionDiscoverer;
    private readonly AutoScalingActivityDiscoverer _autoScalingActivityDiscoverer;

    public AwsComputeDiscoveryService(
        AwsClientFactory clientFactory,
        AwsIdentityService identityService,
        Ec2InstanceDiscoverer instanceDiscoverer,
        EbsVolumeDiscoverer volumeDiscoverer,
        EbsSnapshotDiscoverer snapshotDiscoverer,
        AmiDiscoverer amiDiscoverer,
        Ec2KeyPairDiscoverer keyPairDiscoverer,
        LaunchTemplateDiscoverer launchTemplateDiscoverer,
        AutoScalingGroupDiscoverer autoScalingGroupDiscoverer,
        LaunchConfigurationDiscoverer launchConfigurationDiscoverer,
        AutoScalingPolicyDiscoverer autoScalingPolicyDiscoverer,
        AutoScalingScheduledActionDiscoverer scheduledActionDiscoverer,
        AutoScalingActivityDiscoverer autoScalingActivityDiscoverer)
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
        _launchConfigurationDiscoverer = launchConfigurationDiscoverer;
        _autoScalingPolicyDiscoverer = autoScalingPolicyDiscoverer;
        _scheduledActionDiscoverer = scheduledActionDiscoverer;
        _autoScalingActivityDiscoverer = autoScalingActivityDiscoverer;
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
            LaunchConfigurations = regionalResults
                .SelectMany(result => result.LaunchConfigurations)
                .OrderBy(item => item.Region)
                .ThenBy(item => item.LaunchConfigurationName)
                .ToList(),
            AutoScalingPolicies = regionalResults
                .SelectMany(result => result.AutoScalingPolicies)
                .OrderBy(policy => policy.Region)
                .ThenBy(policy => policy.AutoScalingGroupName)
                .ThenBy(policy => policy.PolicyName)
                .ToList(),
            ScheduledActions = regionalResults
                .SelectMany(result => result.ScheduledActions)
                .OrderBy(action => action.Region)
                .ThenBy(action => action.AutoScalingGroupName)
                .ThenBy(action => action.ScheduledActionName)
                .ToList(),
            AutoScalingActivities = regionalResults
                .SelectMany(result => result.AutoScalingActivities)
                .OrderByDescending(activity => activity.StartedAt)
                .ThenBy(activity => activity.Region)
                .ThenBy(activity => activity.AutoScalingGroupName)
                .ThenBy(activity => activity.ActivityId)
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

            var launchConfigurationsTask =
                _launchConfigurationDiscoverer.DiscoverAsync(
                    autoScalingClient,
                    region,
                    cancellationToken);

            var autoScalingPoliciesTask =
                _autoScalingPolicyDiscoverer.DiscoverAsync(
                    autoScalingClient,
                    region,
                    cancellationToken);

            var scheduledActionsTask =
                _scheduledActionDiscoverer.DiscoverAsync(
                    autoScalingClient,
                    region,
                    cancellationToken);

            var autoScalingActivitiesTask =
                _autoScalingActivityDiscoverer.DiscoverAsync(
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
                autoScalingGroupsTask,
                launchConfigurationsTask,
                autoScalingPoliciesTask,
                scheduledActionsTask,
                autoScalingActivitiesTask);

            return new RegionalComputeResult
            {
                Instances = await instancesTask,
                Volumes = await volumesTask,
                Snapshots = await snapshotsTask,
                Images = await imagesTask,
                KeyPairs = await keyPairsTask,
                LaunchTemplates = await launchTemplatesTask,
                AutoScalingGroups = await autoScalingGroupsTask,
                LaunchConfigurations = await launchConfigurationsTask,
                AutoScalingPolicies = await autoScalingPoliciesTask,
                ScheduledActions = await scheduledActionsTask,
                AutoScalingActivities = await autoScalingActivitiesTask
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

        public IReadOnlyList<AwsLaunchConfigurationInfo>
            LaunchConfigurations
        { get; init; } =
            Array.Empty<AwsLaunchConfigurationInfo>();

        public IReadOnlyList<AwsAutoScalingPolicyInfo>
            AutoScalingPolicies
        { get; init; } =
            Array.Empty<AwsAutoScalingPolicyInfo>();

        public IReadOnlyList<AwsAutoScalingScheduledActionInfo>
            ScheduledActions
        { get; init; } =
            Array.Empty<AwsAutoScalingScheduledActionInfo>();

        public IReadOnlyList<AwsAutoScalingActivityInfo>
            AutoScalingActivities
        { get; init; } =
            Array.Empty<AwsAutoScalingActivityInfo>();

        public IReadOnlyList<string> Warnings { get; init; } =
            Array.Empty<string>();
    }
}
