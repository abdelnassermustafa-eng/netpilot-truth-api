using TruthApi.Models.Platform.Catalog;

namespace TruthApi.Services.Platform.Catalog;

/// <summary>
/// Contributes provider, domain, and resource-type capabilities to the
/// platform catalog.
///
/// Future provider or service modules register an implementation of this
/// interface. The dashboard requires no structural change.
/// </summary>
public interface IPlatformCatalogContributor
{
    PlatformProviderDefinition GetProviderDefinition();
}
