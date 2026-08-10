using TruthApi.Models.Platform.Catalog;

namespace TruthApi.Services.Platform.Catalog;

public sealed class PlatformCatalogService
{
    private readonly IReadOnlyList<IPlatformCatalogContributor>
        _contributors;

    public PlatformCatalogService(
        IEnumerable<IPlatformCatalogContributor> contributors)
    {
        _contributors = contributors.ToList();
    }

    public PlatformCatalog GetCatalog()
    {
        var providers = _contributors
            .Select(contributor =>
                contributor.GetProviderDefinition())
            .GroupBy(
                provider => provider.Id,
                StringComparer.OrdinalIgnoreCase)
            .Select(MergeProviderGroup)
            .OrderBy(provider => provider.DisplayName)
            .ToList();

        return new PlatformCatalog
        {
            Providers = providers,
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }

    private static PlatformProviderDefinition MergeProviderGroup(
        IGrouping<string, PlatformProviderDefinition> group)
    {
        var first = group.First();

        var domains = group
            .SelectMany(provider => provider.Domains)
            .GroupBy(
                domain => domain.Id,
                StringComparer.OrdinalIgnoreCase)
            .Select(domainGroup =>
            {
                var domain = domainGroup.First();

                var resourceTypes = domainGroup
                    .SelectMany(item => item.ResourceTypes)
                    .GroupBy(
                        resource => resource.Id,
                        StringComparer.OrdinalIgnoreCase)
                    .Select(resourceGroup => resourceGroup.First())
                    .OrderBy(resource => resource.DisplayName)
                    .ToList();

                return new PlatformDomainDefinition
                {
                    Id = domain.Id,
                    DisplayName = domain.DisplayName,
                    IconKey = domain.IconKey,
                    AccentKey = domain.AccentKey,
                    ResourceTypes = resourceTypes
                };
            })
            .OrderBy(domain => domain.DisplayName)
            .ToList();

        return new PlatformProviderDefinition
        {
            Id = first.Id,
            DisplayName = first.DisplayName,
            IconKey = first.IconKey,
            AccentKey = first.AccentKey,
            Domains = domains
        };
    }
}
