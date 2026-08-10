namespace TruthApi.Models.Platform.Operations;

public sealed class OperationVerificationResult
{
    public bool IsVerified { get; init; }

    public string ExpectedState { get; init; } = "";

    public string ObservedState { get; init; } = "";

    public IReadOnlyList<string> Differences { get; init; } =
        Array.Empty<string>();
}
