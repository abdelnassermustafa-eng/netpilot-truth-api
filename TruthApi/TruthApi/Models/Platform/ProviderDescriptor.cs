namespace TruthApi.Models.Platform;

/// <summary>
/// Describes a source of resources or knowledge within the workbench.
/// </summary>
public sealed class ProviderDescriptor
{
    public string ProviderId { get; init; } = "";

    public string Name { get; init; } = "";

    public string Version { get; init; } = "";

    public ProviderSourceType SourceType { get; init; }

    public bool RequiresNetwork { get; init; }

    public bool SupportsDiscovery { get; init; }

    public bool SupportsSearch { get; init; }

    public bool SupportsOperations { get; init; }

    public bool IsReadOnly { get; init; }

    public IReadOnlyList<string> Domains { get; init; } =
        Array.Empty<string>();
}
