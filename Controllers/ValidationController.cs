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
    private readonly ComputeValidator _computeValidator = new();
    private readonly StorageValidator _storageValidator = new();

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

    [HttpPost("all")]
    public async Task<IActionResult> ValidateAll()
    {
        var networkResults = await _validationService.ValidateNetworkAsync();
        var computeResults = _computeValidator.Run();
        var storageResults = _storageValidator.Run();

        var allResults = networkResults
            .Concat(computeResults)
            .Concat(storageResults)
            .ToList();

        var total = allResults.Count;
        var pass = allResults.Count(r => r.Status == "PASS");
        var fail = allResults.Count(r => r.Status == "FAIL");
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
            Results = allResults
        };

        return Ok(new ApiResponse<ValidationReport>
        {
            Success = true,
            Data = report,
            Timestamp = DateTime.UtcNow
        });
    }

}

