namespace TruthApi.Models.Platform.Operations;

public sealed class OperationAuditRecord
{
    public string AuditId { get; init; } =
        Guid.NewGuid().ToString("N");

    public string OperationId { get; init; } = "";

    public string PlanId { get; init; } = "";

    public string RequestedBy { get; init; } = "";

    public OperationStatus Status { get; init; }

    public DateTimeOffset StartedAt { get; init; }

    public DateTimeOffset CompletedAt { get; init; }

    public IReadOnlyList<string> Messages { get; init; } =
        Array.Empty<string>();
}
