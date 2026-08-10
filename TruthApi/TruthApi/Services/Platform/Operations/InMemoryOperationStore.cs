using System.Collections.Concurrent;
using TruthApi.Models.Platform.Operations;
using PlatformOperation =
    TruthApi.Models.Platform.Operations.Operation;

namespace TruthApi.Services.Platform.Operations;

/// <summary>
/// Thread-safe development implementation of the Operation Store.
///
/// This implementation is process-local and is not durable across
/// application restarts.
/// </summary>
public sealed class InMemoryOperationStore : IOperationStore
{
    private readonly ConcurrentDictionary<string, StoredOperation>
        _operations =
            new(StringComparer.OrdinalIgnoreCase);

    public Task CreateAsync(
        PlatformOperation operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        cancellationToken.ThrowIfCancellationRequested();

        var stored = new StoredOperation(
            operation,
            operation.Version);

        if (!_operations.TryAdd(operation.OperationId, stored))
        {
            throw new InvalidOperationException(
                $"Operation '{operation.OperationId}' already exists.");
        }

        return Task.CompletedTask;
    }

    public Task<PlatformOperation?> GetAsync(
        string operationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationId);
        cancellationToken.ThrowIfCancellationRequested();

        _operations.TryGetValue(
            operationId,
            out var stored);

        return Task.FromResult(stored?.Operation);
    }

    public Task<IReadOnlyList<PlatformOperation>> ListAsync(
        OperationStatus? status = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip));
        }

        if (take is < 1 or > 1000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(take),
                "Take must be between 1 and 1000.");
        }

        var query = _operations.Values
            .Select(item => item.Operation);

        if (status.HasValue)
        {
            query = query.Where(
                operation => operation.Status == status.Value);
        }

        IReadOnlyList<PlatformOperation> results = query
            .OrderByDescending(operation => operation.CreatedAt)
            .ThenBy(operation => operation.OperationId)
            .Skip(skip)
            .Take(take)
            .ToList();

        return Task.FromResult(results);
    }

    public Task SaveAsync(
        PlatformOperation operation,
        long expectedVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        cancellationToken.ThrowIfCancellationRequested();

        while (true)
        {
            if (!_operations.TryGetValue(
                    operation.OperationId,
                    out var existing))
            {
                throw new OperationNotFoundException(
                    operation.OperationId);
            }

            if (existing.Version != expectedVersion)
            {
                throw new OperationConcurrencyException(
                    operation.OperationId,
                    expectedVersion,
                    existing.Version);
            }

            var replacement = new StoredOperation(
                operation,
                operation.Version);

            if (_operations.TryUpdate(
                    operation.OperationId,
                    replacement,
                    existing))
            {
                return Task.CompletedTask;
            }
        }
    }

    private sealed record StoredOperation(
        PlatformOperation Operation,
        long Version);
}
