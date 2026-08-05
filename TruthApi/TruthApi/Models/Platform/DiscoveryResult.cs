namespace TruthApi.Models.Platform;

/// <summary>
/// Represents a provider discovery response.
/// </summary>
public sealed class DiscoveryResult
{
    public ProviderDescriptor Provider { get; init; } = new();

    public IReadOnlyList<WorkbenchItem> Items { get; init; } =
        Array.Empty<WorkbenchItem>();

    public string? ContinuationToken { get; init; }

    public DateTimeOffset DiscoveredAt { get; init; } =
        DateTimeOffset.UtcNow;

    public IReadOnlyList<string> Warnings { get; init; } =
        Array.Empty<string>();
}
