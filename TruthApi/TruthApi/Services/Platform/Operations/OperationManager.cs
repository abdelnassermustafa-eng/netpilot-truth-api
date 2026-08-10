using TruthApi.Models.Platform.Operations;
using PlatformOperation =
    TruthApi.Models.Platform.Operations.Operation;

namespace TruthApi.Services.Platform.Operations;

/// <summary>
/// Public façade for the complete infrastructure-operation subsystem.
///
/// Controllers and future backend consumers use this manager rather
/// than accessing OperationStore or OperationService directly.
/// </summary>
public sealed class OperationManager : IOperationManager
{
    private readonly IOperationStore _store;
    private readonly OperationService _operationService;

    public OperationManager(
        IOperationStore store,
        OperationService operationService)
    {
        _store = store;
        _operationService = operationService;
    }

    public async Task<PlatformOperation> CreateAsync(
        OperationRequest request,
        OperationStateSnapshot? currentState = null,
        OperationStateSnapshot? desiredState = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var operation = PlatformOperation.Create(request);

        if (currentState is not null)
        {
            operation.CaptureCurrentState(currentState);
        }

        if (desiredState is not null)
        {
            operation.DefineDesiredState(desiredState);
        }

        await _store.CreateAsync(
            operation,
            cancellationToken);

        return operation;
    }

    public async Task<PlatformOperation> ExecuteAsync(
        string operationId,
        CancellationToken cancellationToken = default)
    {
        var operation = await RequireAsync(
            operationId,
            cancellationToken);

        if (operation.Status ==
            OperationStatus.AwaitingConfirmation)
        {
            throw new InvalidOperationException(
                $"Operation '{operationId}' requires confirmation.");
        }

        return await _operationService.ExecuteAsync(
            operation,
            SaveCheckpointAsync,
            cancellationToken);
    }

    public Task<PlatformOperation?> GetAsync(
        string operationId,
        CancellationToken cancellationToken = default)
    {
        return _store.GetAsync(
            operationId,
            cancellationToken);
    }

    public Task<IReadOnlyList<PlatformOperation>> ListAsync(
        OperationStatus? status = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        return _store.ListAsync(
            status,
            skip,
            take,
            cancellationToken);
    }

    public async Task<PlatformOperation> ApproveAsync(
        string operationId,
        string approvedBy,
        string comment = "",
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(approvedBy);

        var operation = await RequireAsync(
            operationId,
            cancellationToken);

        var expectedVersion = operation.Version;

        operation.ApprovePlan(
            approvedBy,
            comment);

        await _store.SaveAsync(
            operation,
            expectedVersion,
            cancellationToken);

        return operation;
    }

    public async Task<PlatformOperation> RejectAsync(
        string operationId,
        string rejectedBy,
        string comment,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rejectedBy);
        ArgumentException.ThrowIfNullOrWhiteSpace(comment);

        var operation = await RequireAsync(
            operationId,
            cancellationToken);

        var expectedVersion = operation.Version;

        operation.RejectPlan(
            rejectedBy,
            comment);

        await _store.SaveAsync(
            operation,
            expectedVersion,
            cancellationToken);

        return operation;
    }

    public async Task<PlatformOperation> CancelAsync(
        string operationId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var operation = await RequireAsync(
            operationId,
            cancellationToken);

        if (operation.IsTerminal)
        {
            return operation;
        }

        var expectedVersion = operation.Version;

        operation.Cancel(reason.Trim());

        await _store.SaveAsync(
            operation,
            expectedVersion,
            cancellationToken);

        return operation;
    }

    private Task SaveCheckpointAsync(
        PlatformOperation operation,
        long expectedVersion,
        CancellationToken cancellationToken)
    {
        return _store.SaveAsync(
            operation,
            expectedVersion,
            cancellationToken);
    }

    private async Task<PlatformOperation> RequireAsync(
        string operationId,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationId);

        return await _store.GetAsync(
                   operationId,
                   cancellationToken)
               ?? throw new OperationNotFoundException(operationId);
    }
}
