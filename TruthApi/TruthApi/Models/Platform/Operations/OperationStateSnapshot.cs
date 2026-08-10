namespace TruthApi.Models.Platform.Operations;

/// <summary>
/// Represents infrastructure state observed or requested at a specific
/// point in an operation lifecycle.
///
/// Json contains a provider-independent serialized state document.
/// </summary>
public sealed class OperationStateSnapshot
{
    public string Json { get; init; } = "{}";

    public DateTimeOffset CapturedAt { get; init; } =
        DateTimeOffset.UtcNow;

    public string Source { get; init; } = "";
}
