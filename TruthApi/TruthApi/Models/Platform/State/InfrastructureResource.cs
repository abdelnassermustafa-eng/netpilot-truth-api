namespace TruthApi.Models.Platform.State;

public sealed class InfrastructureResource
{
    public string ProviderId { get; init; } = "";

    public string AccountId { get; init; } = "";

    public string DomainId { get; init; } = "";

    public string ResourceType { get; init; } = "";

    public string ResourceId { get; init; } = "";

    public string NativeId { get; init; } = "";

    public string DisplayName { get; init; } = "";

    public string State { get; init; } = "";

    public string Location { get; init; } = "";

    public string AvailabilityZone { get; init; } = "";

    public string Arn { get; init; } = "";

    public string IconKey { get; init; } = "resource";

    public string AccentKey { get; init; } = "default";

    public IReadOnlyDictionary<string, string> Properties
    { get; init; } =
        new Dictionary<string, string>();

    public IReadOnlyDictionary<string, string> Tags
    { get; init; } =
        new Dictionary<string, string>();

    public IReadOnlyList<InfrastructureRelationship> Relationships
    { get; init; } =
        Array.Empty<InfrastructureRelationship>();

    public IReadOnlyList<string> Capabilities { get; init; } =
        Array.Empty<string>();

    public DateTimeOffset DiscoveredAt { get; init; } =
        DateTimeOffset.UtcNow;
}
