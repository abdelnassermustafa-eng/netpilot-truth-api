namespace TruthApi.Services.Platform.Operations;

public sealed class OperationConcurrencyException : Exception
{
    public OperationConcurrencyException(
        string operationId,
        long expectedVersion,
        long actualVersion)
        : base(
            $"Operation '{operationId}' has version {actualVersion}; " +
            $"expected version {expectedVersion}.")
    {
        OperationId = operationId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }

    public string OperationId { get; }

    public long ExpectedVersion { get; }

    public long ActualVersion { get; }
}
