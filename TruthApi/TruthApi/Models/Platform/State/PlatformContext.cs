namespace TruthApi.Models.Platform.State;

public sealed class PlatformContext
{
    public string ProviderId { get; init; } = "";

    public string ProviderName { get; init; } = "";

    public string AccountId { get; init; } = "";

    public string AccountName { get; init; } = "";

    public string IdentityId { get; init; } = "";

    public string IdentityArn { get; init; } = "";

    public string DefaultLocation { get; init; } = "";

    public IReadOnlyList<string> Locations { get; init; } =
        Array.Empty<string>();
}
