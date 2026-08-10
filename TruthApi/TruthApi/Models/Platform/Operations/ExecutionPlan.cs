namespace TruthApi.Models.Platform.Operations;

public sealed class ExecutionPlan
{
    public string PlanId { get; init; } =
        Guid.NewGuid().ToString("N");

    public string OperationId { get; init; } = "";

    public string Provider { get; init; } = "";

    public string Service { get; init; } = "";

    public string ResourceType { get; init; } = "";

    public IReadOnlyList<ExecutionStep> Steps { get; init; } =
        Array.Empty<ExecutionStep>();

    public IReadOnlyList<string> ImpactWarnings { get; init; } =
        Array.Empty<string>();

    public bool RequiresConfirmation =>
        Steps.Any(step => step.IsDestructive);

    public DateTimeOffset CreatedAt { get; init; } =
        DateTimeOffset.UtcNow;
}
