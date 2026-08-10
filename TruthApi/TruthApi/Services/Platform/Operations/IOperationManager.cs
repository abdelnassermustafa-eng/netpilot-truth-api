using TruthApi.Models.Platform.Operations;
using PlatformOperation =
    TruthApi.Models.Platform.Operations.Operation;

namespace TruthApi.Services.Platform.Operations;

/// <summary>
/// Public façade for creating, executing, querying, and cancelling
/// infrastructure operations.
/// </summary>
public interface IOperationManager
{
    Task<PlatformOperation> CreateAsync(
        OperationRequest request,
        OperationStateSnapshot? currentState = null,
        OperationStateSnapshot? desiredState = null,
        CancellationToken cancellationToken = default);

    Task<PlatformOperation> ExecuteAsync(
        string operationId,
        CancellationToken cancellationToken = default);

    Task<PlatformOperation?> GetAsync(
        string operationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlatformOperation>> ListAsync(
        OperationStatus? status = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default);

    Task<PlatformOperation> ApproveAsync(
        string operationId,
        string approvedBy,
        string comment = "",
        CancellationToken cancellationToken = default);

    Task<PlatformOperation> RejectAsync(
        string operationId,
        string rejectedBy,
        string comment,
        CancellationToken cancellationToken = default);

    Task<PlatformOperation> CancelAsync(
        string operationId,
        string reason,
        CancellationToken cancellationToken = default);
}
