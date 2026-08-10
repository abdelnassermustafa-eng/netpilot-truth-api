using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruthApi.Models;
using TruthApi.Models.Platform.Catalog;
using TruthApi.Services.Platform.Catalog;

namespace TruthApi.Controllers;

[ApiController]
[Route("api/v2/platform/catalog")]
[Authorize(Roles = "Admin,Viewer")]
public sealed class PlatformCatalogController : ControllerBase
{
    private readonly PlatformCatalogService _catalogService;

    public PlatformCatalogController(
        PlatformCatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<PlatformCatalog>),
        StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<PlatformCatalog>> GetCatalog()
    {
        return Ok(new ApiResponse<PlatformCatalog>
        {
            Success = true,
            Data = _catalogService.GetCatalog(),
            Timestamp = DateTime.UtcNow
        });
    }
}
