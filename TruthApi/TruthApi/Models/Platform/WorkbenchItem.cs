namespace TruthApi.Models.Platform;

/// <summary>
/// Provides a universal representation of an infrastructure, knowledge,
/// automation, protection, project, cost, or tool item.
///
/// Domain services should retain strongly typed internal models and convert
/// them into this envelope only when a common platform representation is
/// required.
/// </summary>
public sealed class WorkbenchItem
{
    public string Id { get; init; } = "";

    public string Name { get; init; } = "";

    public string Kind { get; init; } = "";

    public string Domain { get; init; } = "";

    public string Category { get; init; } = "";

    public string ProviderId { get; init; } = "";

    public string Source { get; init; } = "";

    public string Status { get; init; } = "";

    public string SchemaVersion { get; init; } = "1.0";

    public string? ParentId { get; init; }

    public string? ProjectId { get; init; }

    public string? AccountId { get; init; }

    public string? Region { get; init; }

    public string? Scope { get; init; }

    public IReadOnlyDictionary<string, string> Tags { get; init; } =
        new Dictionary<string, string>();

    public IReadOnlyDictionary<string, object?> Properties { get; init; } =
        new Dictionary<string, object?>();

    public IReadOnlyList<WorkbenchRelationship> Relationships { get; init; } =
        Array.Empty<WorkbenchRelationship>();

    public IReadOnlyList<WorkbenchCapability> Capabilities { get; init; } =
        Array.Empty<WorkbenchCapability>();

    public DateTimeOffset DiscoveredAt { get; init; } =
        DateTimeOffset.UtcNow;
}
