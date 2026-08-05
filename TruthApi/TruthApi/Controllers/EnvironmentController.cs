using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruthApi.Models;
using TruthApi.Models.Aws;
using TruthApi.Services.Aws;

namespace TruthApi.Controllers;

/// <summary>
/// Provides information about the AWS environment currently used by TruthApi.
/// </summary>
[ApiController]
[Route("api/v2/environment")]
[Authorize(Roles = "Admin,Viewer")]
public sealed class EnvironmentController : ControllerBase
{
    private readonly AwsResourceDiscoveryService _discoveryService;

    public EnvironmentController(
        AwsResourceDiscoveryService discoveryService)
    {
        _discoveryService = discoveryService;
    }

    /// <summary>
    /// Returns the current AWS identity, default Region, and visible Regions.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<AwsEnvironmentInfo>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<AwsEnvironmentInfo>>>
        GetEnvironment(
            [FromQuery] bool includeDisabledRegions = true,
            CancellationToken cancellationToken = default)
    {
        var environment =
            await _discoveryService.DiscoverEnvironmentAsync(
                includeDisabledRegions,
                cancellationToken);

        return Ok(new ApiResponse<AwsEnvironmentInfo>
        {
            Success = true,
            Data = environment,
            Timestamp = DateTime.UtcNow
        });
    }
}
