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
    private readonly AwsEc2Service _ec2Service;

    public ValidationController(
        NetworkValidationService validationService,
        AwsEc2Service ec2Service)
    {
        _validationService = validationService;
        _ec2Service = ec2Service;
    }

    [HttpGet("network")]
    public async Task<IActionResult> ValidateNetwork()
    {
        var results = await _validationService.ValidateNetworkAsync();

        var total = results.Count;
        var pass = results.Count(r => r.Status == "PASS");
        var fail = results.Count(r => r.Status == "FAIL");

        var score = total == 0 ? 100 : (int)((double)pass / total * 100);

        // Phase 6.2 — resource inventories
        var vpcRows = await _ec2Service.GetVpcResourceRowsAsync();
        var subnetRows = await _ec2Service.GetSubnetResourceRowsAsync();
        var routeTableRows = await _ec2Service.GetRouteTableResourceRowsAsync();

        var report = new ValidationReport
        {
            Summary = new ValidationSummary
            {
                Total = total,
                Pass = pass,
                Fail = fail,
                Score = score
            },
            Results = results,

            // Phase 6.2 additions
            Vpcs = vpcRows,
            Subnets = subnetRows,
            RouteTables = routeTableRows
        };

        return Ok(new ApiResponse<ValidationReport>
        {
            Success = true,
            Data = report,
            Timestamp = DateTime.UtcNow
        });
    }
}
