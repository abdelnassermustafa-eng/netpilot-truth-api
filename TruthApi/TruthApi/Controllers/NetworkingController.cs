using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruthApi.Models;
using TruthApi.Models.Aws.Networking;
using TruthApi.Services.Aws.Networking;

namespace TruthApi.Controllers;

/// <summary>
/// Version 2 networking inventory.
/// </summary>
[ApiController]
[Route("api/v2/networking")]
[Authorize(Roles = "Admin,Viewer")]
public sealed class NetworkingController : ControllerBase
{
    private readonly AwsNetworkingDiscoveryService _networking;

    public NetworkingController(
        AwsNetworkingDiscoveryService networking)
    {
        _networking = networking;
    }

    /// <summary>
    /// Returns live networking inventory.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<AwsNetworkingInventory>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AwsNetworkingInventory>>>
        GetNetworking(
            [FromQuery] List<string>? region,
            CancellationToken cancellationToken)
    {
        var inventory =
            await _networking.DiscoverAsync(
                region,
                cancellationToken);

        return Ok(
            new ApiResponse<AwsNetworkingInventory>
            {
                Success = true,
                Data = inventory,
                Timestamp = DateTime.UtcNow
            });
    }
}
