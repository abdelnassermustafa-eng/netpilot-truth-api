namespace TruthApi.Models.Platform.Operations;

/// <summary>
/// Represents the current progress of an infrastructure operation.
/// </summary>
public sealed class OperationProgress
{
    public int CompletedSteps { get; init; }

    public int TotalSteps { get; init; }

    public int Percentage =>
        TotalSteps <= 0
            ? 0
            : Math.Clamp(
                (int)Math.Round(
                    CompletedSteps * 100d / TotalSteps),
                0,
                100);

    public string CurrentStep { get; init; } = "";

    public string Message { get; init; } = "";
}
