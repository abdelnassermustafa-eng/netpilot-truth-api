namespace TruthApi.Models.Platform.Operations;

public sealed class ExecutionStep
{
    public int Order { get; init; }

    public string Name { get; init; } = "";

    public string Description { get; init; } = "";

    public string Operation { get; init; } = "";

    public string? ResourceId { get; init; }

    public bool IsDestructive { get; init; }

    public bool IsReversible { get; init; }

    public IReadOnlyDictionary<string, string> Parameters
    { get; init; } =
        new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);
}
