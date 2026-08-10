namespace TruthApi.Models.Platform.Operations;

/// <summary>
/// Represents one permanent event in an operation lifecycle.
/// </summary>
public sealed class OperationTimelineEntry
{
    public DateTimeOffset OccurredAt { get; init; } =
        DateTimeOffset.UtcNow;

    public OperationStatus Status { get; init; }

    public string EventType { get; init; } = "";

    public string Message { get; init; } = "";

    public IReadOnlyDictionary<string, string> Metadata
    { get; init; } =
        new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);
}
