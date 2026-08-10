namespace TruthApi.Models.Platform.State;

public sealed class InfrastructureState
{
    public PlatformContext Context { get; init; } = new();

    public IReadOnlyList<InfrastructureDomain> Domains
    { get; init; } =
        Array.Empty<InfrastructureDomain>();

    public IReadOnlyList<InfrastructureResource> Resources
    { get; init; } =
        Array.Empty<InfrastructureResource>();

    public IReadOnlyList<InfrastructureRelationship> Relationships
    { get; init; } =
        Array.Empty<InfrastructureRelationship>();

    public IReadOnlyList<string> Warnings { get; init; } =
        Array.Empty<string>();

    public DateTimeOffset DiscoveredAt { get; init; } =
        DateTimeOffset.UtcNow;

    public int TotalResourceCount => Resources.Count;
}
