using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruthApi.Models;
using TruthApi.Models.Aws.LoadBalancing;
using TruthApi.Services.Aws.LoadBalancing;

namespace TruthApi.Controllers;

[ApiController]
[Route("api/v2/load-balancing")]
[Authorize(Roles = "Admin,Viewer")]
public sealed class LoadBalancingController : ControllerBase
{
    private readonly AwsLoadBalancingDiscoveryService _discovery;

    public LoadBalancingController(
        AwsLoadBalancingDiscoveryService discovery)
    {
        _discovery = discovery;
    }

    [HttpGet]
    public async Task<
        ActionResult<
            ApiResponse<AwsLoadBalancingInventory>>>
        GetLoadBalancing(
            [FromQuery] List<string>? region,
            CancellationToken cancellationToken)
    {
        var inventory = await _discovery.DiscoverAsync(
            region,
            cancellationToken);

        return Ok(
            new ApiResponse<AwsLoadBalancingInventory>
            {
                Success = true,
                Data = inventory,
                Timestamp = DateTime.UtcNow
            });
    }
}
