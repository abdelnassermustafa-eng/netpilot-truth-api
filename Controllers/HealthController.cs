using Microsoft.AspNetCore.Mvc;
using TruthApi.Services;
using TruthApi.Models;

namespace TruthApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly HealthService _healthService;

        public HealthController(HealthService healthService)
        {
            _healthService = healthService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var healthData = _healthService.GetHealthStatus();

            var response = new ApiResponse<object>
            {
                Success = true,
                Data = healthData
            };

            return Ok(response);
        }

    }
}
