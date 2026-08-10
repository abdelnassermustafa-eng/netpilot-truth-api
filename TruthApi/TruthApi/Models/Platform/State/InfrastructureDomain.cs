namespace TruthApi.Models.Platform.State;

public sealed class InfrastructureDomain
{
    public string Id { get; init; } = "";

    public string DisplayName { get; init; } = "";

    public string IconKey { get; init; } = "resource";

    public string AccentKey { get; init; } = "default";

    public int ResourceCount { get; init; }

    public IReadOnlyList<string> ResourceTypes { get; init; } =
        Array.Empty<string>();
}
