using TruthApi.Models.Platform.Operations;
using PlatformOperation =
    TruthApi.Models.Platform.Operations.Operation;

namespace TruthApi.Services.Platform.Operations;

/// <summary>
/// Stores permanent, queryable infrastructure operations.
///
/// Implementations must enforce optimistic concurrency using the
/// aggregate Version.
/// </summary>
public interface IOperationStore
{
    Task CreateAsync(
        PlatformOperation operation,
        CancellationToken cancellationToken = default);

    Task<PlatformOperation?> GetAsync(
        string operationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PlatformOperation>> ListAsync(
        OperationStatus? status = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        PlatformOperation operation,
        long expectedVersion,
        CancellationToken cancellationToken = default);
}
