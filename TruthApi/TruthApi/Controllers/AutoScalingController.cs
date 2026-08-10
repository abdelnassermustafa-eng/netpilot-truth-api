using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruthApi.Models;
using TruthApi.Models.Aws.Compute;
using TruthApi.Services.Aws.Compute;

namespace TruthApi.Controllers;

[ApiController]
[Route("api/v2/auto-scaling")]
[Authorize(Roles = "Admin,Viewer")]
public sealed class AutoScalingController : ControllerBase
{
    private readonly AwsComputeDiscoveryService _computeDiscovery;

    public AutoScalingController(
        AwsComputeDiscoveryService computeDiscovery)
    {
        _computeDiscovery = computeDiscovery;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<AwsComputeInventory>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<AwsComputeInventory>>>
        GetAutoScaling(
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
