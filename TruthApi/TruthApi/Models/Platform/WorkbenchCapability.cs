namespace TruthApi.Models.Platform;

/// <summary>
/// Describes an operation or interaction supported by a workbench item.
/// </summary>
public sealed class WorkbenchCapability
{
    public string Name { get; init; } = "";

    public string Operation { get; init; } = "";

    public string HttpMethod { get; init; } = "";

    public bool IsAvailable { get; init; }

    public bool RequiresConfirmation { get; init; }

    public bool SupportsDryRun { get; init; }

    public bool RequiresRestorePoint { get; init; }

    public string UnavailableReason { get; init; } = "";
}
