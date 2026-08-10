namespace TruthApi.Models.Platform.Catalog;

public sealed class PlatformDomainDefinition
{
    public string Id { get; init; } = "";

    public string DisplayName { get; init; } = "";

    public string IconKey { get; init; } = "resource";

    public string AccentKey { get; init; } = "default";

    public IReadOnlyList<PlatformResourceTypeDefinition> ResourceTypes
    { get; init; } =
        Array.Empty<PlatformResourceTypeDefinition>();
}
