namespace TruthApi.Models.Platform.Operations;

/// <summary>
/// Represents the lifecycle state of an infrastructure operation.
/// </summary>
public enum OperationStatus
{
    Pending,
    Validating,
    ValidationFailed,
    Validated,
    Planning,
    Planned,
    AwaitingConfirmation,
    Executing,
    Verifying,
    Succeeded,
    PartiallySucceeded,
    Failed,
    Rejected,
    Cancelled
}
