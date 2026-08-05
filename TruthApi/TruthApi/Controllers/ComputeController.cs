using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruthApi.Models;
using TruthApi.Models.Aws.Compute;
using TruthApi.Services.Aws.Compute;

namespace TruthApi.Controllers;

/// <summary>
/// Version 2 AWS compute inventory.
/// </summary>
[ApiController]
[Route("api/v2/compute")]
[Authorize(Roles = "Admin,Viewer")]
public sealed class ComputeController : ControllerBase
{
    private readonly AwsComputeDiscoveryService _computeDiscovery;

    public ComputeController(
        AwsComputeDiscoveryService computeDiscovery)
    {
        _computeDiscovery = computeDiscovery;
    }

    /// <summary>
    /// Returns live EC2 compute inventory for one or more Regions.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<AwsComputeInventory>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<AwsComputeInventory>>>
        GetCompute(
            [FromQuery] List<string>? region,
            CancellationToken cancellationToken)
    {
        var inventory = await _computeDiscovery.DiscoverAsync(
            region,
            cancellationToken);

        return Ok(new ApiResponse<AwsComputeInventory>
        {
            Success = true,
            Data = inventory,
            Timestamp = DateTime.UtcNow
        });
    }
}
