using TruthApi.Models.Platform.State;

namespace TruthApi.Services.Platform.State.Adapters;

public interface IInfrastructureResourceAdapter<in TInventory>
{
    IReadOnlyList<InfrastructureResource> Adapt(
        TInventory inventory,
        DateTimeOffset discoveredAt);
}
