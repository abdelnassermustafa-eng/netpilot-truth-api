namespace TruthApi.Models.Platform;

/// <summary>
/// Defines the scope of a provider discovery operation.
/// </summary>
public sealed class DiscoveryRequest
{
    public IReadOnlyList<string> ProviderIds { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> Domains { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> Categories { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> Kinds { get; init; } =
        Array.Empty<string>();

    public IReadOnlyList<string> Regions { get; init; } =
        Array.Empty<string>();

    public string? AccountId { get; init; }

    public string? ProjectId { get; init; }

    public string? SearchText { get; init; }

    public bool IncludeRelationships { get; init; } = true;

    public bool IncludeCapabilities { get; init; } = true;

    public int PageSize { get; init; } = 100;

    public string? ContinuationToken { get; init; }
}
