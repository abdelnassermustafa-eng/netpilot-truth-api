namespace TruthApi.Models.Platform.Catalog;

public sealed class PlatformProviderDefinition
{
    public string Id { get; init; } = "";

    public string DisplayName { get; init; } = "";

    public string IconKey { get; init; } = "cloud";

    public string AccentKey { get; init; } = "default";

    public IReadOnlyList<PlatformDomainDefinition> Domains
    { get; init; } =
        Array.Empty<PlatformDomainDefinition>();
}
