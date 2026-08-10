namespace TruthApi.Models.Platform.Operations;

public sealed class OperationValidationResult
{
    public bool IsValid { get; init; }

    public IReadOnlyList<string> Errors { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> Warnings { get; init; } =
        Array.Empty<string>();

    public static OperationValidationResult Success(
        IReadOnlyList<string>? warnings = null)
    {
        return new OperationValidationResult
        {
            IsValid = true,
            Warnings = warnings ?? Array.Empty<string>()
        };
    }

    public static OperationValidationResult Failure(
        params string[] errors)
    {
        return new OperationValidationResult
        {
            IsValid = false,
            Errors = errors
        };
    }
}
