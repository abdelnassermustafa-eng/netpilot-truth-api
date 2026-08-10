namespace TruthApi.Models.Platform.Operations;

/// <summary>
/// Describes a requested infrastructure state transition.
/// </summary>
public sealed class OperationRequest
{
    public string OperationId { get; init; } =
        Guid.NewGuid().ToString("N");

    public string Provider { get; init; } = "";

    public string Service { get; init; } = "";

    public string ResourceType { get; init; } = "";

    public string? ResourceId { get; init; }

    public string Operation { get; init; } = "";

    public string Region { get; init; } = "";

    public IReadOnlyDictionary<string, string> Parameters
    { get; init; } =
        new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

    public string RequestedBy { get; init; } = "";

    public DateTimeOffset RequestedAt { get; init; } =
        DateTimeOffset.UtcNow;
}
