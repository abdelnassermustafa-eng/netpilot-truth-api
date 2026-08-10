namespace TruthApi.Models.Platform.Operations;

/// <summary>
/// Records the explicit human decision for an operation requiring
/// confirmation.
/// </summary>
public sealed class OperationApprovalRecord
{
    public OperationApprovalDecision Decision { get; init; }

    public string DecidedBy { get; init; } = "";

    public DateTimeOffset DecidedAt { get; init; } =
        DateTimeOffset.UtcNow;

    public string Comment { get; init; } = "";
}
