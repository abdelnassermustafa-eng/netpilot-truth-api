using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruthApi.Models;
using TruthApi.Models.Platform.State;
using TruthApi.Services.Platform.State;

namespace TruthApi.Controllers;

[ApiController]
[Route("api/v2/platform/state")]
[Authorize(Roles = "Admin,Viewer")]
public sealed class PlatformStateController : ControllerBase
{
    private readonly InfrastructureStateService _stateService;

    public PlatformStateController(
        InfrastructureStateService stateService)
    {
        _stateService = stateService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<InfrastructureState>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<ApiResponse<InfrastructureState>>>
        GetState(
            [FromQuery] List<string>? region,
            CancellationToken cancellationToken)
    {
        var state = await _stateService.DiscoverAsync(
            region,
            cancellationToken);

        return Ok(new ApiResponse<InfrastructureState>
        {
            Success = true,
            Data = state,
            Timestamp = DateTime.UtcNow
        });
    }
}
