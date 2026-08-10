namespace TruthApi.Services.Platform.Operations;

public sealed class OperationNotFoundException : Exception
{
    public OperationNotFoundException(string operationId)
        : base($"Operation '{operationId}' was not found.")
    {
        OperationId = operationId;
    }

    public string OperationId { get; }
}
