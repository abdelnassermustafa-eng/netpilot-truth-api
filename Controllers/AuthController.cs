using Microsoft.AspNetCore.Mvc;
using TruthApi.Models;
using TruthApi.Services;

namespace TruthApi.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var token = _authService.Authenticate(request);

            if (token == null)
            {
                return Unauthorized(new ApiResponse<string>
                {
                    Success = false,
                    Data = null,
                    Timestamp = DateTime.UtcNow
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new { token }
            });
        }
    }
}

