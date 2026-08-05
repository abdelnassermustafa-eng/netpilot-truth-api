namespace TruthApi.Models.Platform;

/// <summary>
/// Describes a relationship between two workbench items.
/// </summary>
public sealed class WorkbenchRelationship
{
    public string RelationshipType { get; init; } = "";

    public string TargetId { get; init; } = "";

    public string TargetKind { get; init; } = "";

    public string Direction { get; init; } = "Outgoing";

    public string Description { get; init; } = "";
}
