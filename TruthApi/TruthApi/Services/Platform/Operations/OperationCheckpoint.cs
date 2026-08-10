using PlatformOperation =
    TruthApi.Models.Platform.Operations.Operation;

namespace TruthApi.Services.Platform.Operations;

/// <summary>
/// Persists an Operation after one lifecycle transition.
///
/// expectedVersion is the version that was persisted before the
/// transition occurred.
/// </summary>
public delegate Task OperationCheckpoint(
    PlatformOperation operation,
    long expectedVersion,
    CancellationToken cancellationToken);
