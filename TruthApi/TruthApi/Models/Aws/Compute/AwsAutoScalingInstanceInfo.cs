namespace TruthApi.Models.Aws.Compute;

public sealed class AwsAutoScalingInstanceInfo
{
    public string InstanceId { get; init; } = "";

    public string InstanceType { get; init; } = "";

    public string ImageId { get; init; } = "";

    public string AvailabilityZone { get; init; } = "";

    public string HealthStatus { get; init; } = "";

    public string LifecycleState { get; init; } = "";

    public bool ProtectedFromScaleIn { get; init; }

    public string WeightedCapacity { get; init; } = "";

    public string LaunchConfigurationName { get; init; } = "";

    public string LaunchTemplateId { get; init; } = "";

    public string LaunchTemplateName { get; init; } = "";

    public string LaunchTemplateVersion { get; init; } = "";
}
