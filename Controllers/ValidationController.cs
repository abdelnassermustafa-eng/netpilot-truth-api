using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruthApi.Models;
using TruthApi.Services;

namespace TruthApi.Controllers;

[ApiController]
[Route("api/v1/validate")]
[Authorize(Roles = "Admin,Viewer")]
public class ValidationController : ControllerBase
{
    private readonly NetworkValidationService _validationService;

    public ValidationController(NetworkValidationService validationService)
    {
        _validationService = validationService;
    }

    [HttpGet("network")]
    public async Task<IActionResult> ValidateNetwork()
    {
        var results = await _validationService.ValidateNetworkAsync();

        var total = results.Count;
        var pass = results.Count(r => r.Status == "PASS");
        var fail = results.Count(r => r.Status == "FAIL");

        var score = total == 0 ? 100 : (int)((double)pass / total * 100);

        var report = new ValidationReport
        {
            Summary = new ValidationSummary
            {
                Total = total,
                Pass = pass,
                Fail = fail,
                Score = score
            },
            Results = results
        };

        return Ok(new ApiResponse<ValidationReport>
        {
            Success = true,
            Data = report,
            Timestamp = DateTime.UtcNow
        });
    }

}

