namespace TruthApi.Models.Platform;

/// <summary>
/// Identifies where a provider obtains its information.
/// </summary>
public enum ProviderSourceType
{
    RemoteApi,
    PackagedLocal,
    UserLocal,
    OrganizationRemote
}
