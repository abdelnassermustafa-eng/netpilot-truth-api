namespace TruthApi.Models.Platform.Operations;

public sealed class OperationResult
{
    public string OperationId { get; init; } = "";

    public string PlanId { get; init; } = "";

    public OperationStatus Status { get; init; }

    public OperationValidationResult? Validation { get; init; }

    public OperationVerificationResult? Verification { get; init; }

    public IReadOnlyList<string> Warnings { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> Errors { get; init; } =
        Array.Empty<string>();

    public DateTimeOffset CompletedAt { get; init; } =
        DateTimeOffset.UtcNow;
}
