namespace TruthApi.Models.Platform.State;

public sealed class InfrastructureRelationship
{
    public string Type { get; init; } = "";

    public string SourceResourceId { get; init; } = "";

    public string TargetResourceId { get; init; } = "";

    public string Description { get; init; } = "";
}
