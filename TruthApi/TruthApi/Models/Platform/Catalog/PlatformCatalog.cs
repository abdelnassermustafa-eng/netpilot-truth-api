namespace TruthApi.Models.Platform.Catalog;

public sealed class PlatformCatalog
{
    public IReadOnlyList<PlatformProviderDefinition> Providers
    { get; init; } =
        Array.Empty<PlatformProviderDefinition>();

    public DateTimeOffset GeneratedAt { get; init; } =
        DateTimeOffset.UtcNow;
}
