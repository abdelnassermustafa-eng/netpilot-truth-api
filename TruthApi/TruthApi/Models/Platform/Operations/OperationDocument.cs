namespace TruthApi.Models.Platform.Operations;

/// <summary>
/// Serializable persistence representation of an Operation aggregate.
/// </summary>
public sealed class OperationDocument
{
    public string OperationId { get; init; } = "";

    public string Provider { get; init; } = "";

    public string Service { get; init; } = "";

    public string ResourceType { get; init; } = "";

    public string? ResourceId { get; init; }

    public string OperationName { get; init; } = "";

    public string Region { get; init; } = "";

    public IReadOnlyDictionary<string, string> Parameters
    { get; init; } =
        new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

    public string RequestedBy { get; init; } = "";

    public DateTimeOffset RequestedAt { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }

    public OperationStatus Status { get; init; }

    public long Version { get; init; }

    public OperationStateSnapshot? CurrentState { get; init; }

    public OperationStateSnapshot? DesiredState { get; init; }

    public OperationStateSnapshot? ObservedState { get; init; }

    public OperationValidationResult? Validation { get; init; }

    public ExecutionPlan? Plan { get; init; }

    public OperationApprovalRecord? Approval
    { get; init; }

    public OperationProgress Progress { get; init; } = new();

    public OperationVerificationResult? Verification { get; init; }

    public OperationResult? Result { get; init; }

    public IReadOnlyList<OperationTimelineEntry> Timeline
    { get; init; } =
        Array.Empty<OperationTimelineEntry>();
}
