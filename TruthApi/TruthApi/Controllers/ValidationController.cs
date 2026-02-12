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
    private readonly ComputeValidator _computeValidator;

    public ValidationController(
        NetworkValidationService validationService,
        AwsEc2Service ec2Service,
        ComputeValidator computeValidator)
    {
        _validationService = validationService;
        _ec2Service = ec2Service;
        _computeValidator = computeValidator;
    }

    [HttpGet("network")]
    public async Task<IActionResult> ValidateNetwork()
    {
        var results = await _validationService.ValidateNetworkAsync();

        var total = results.Count;
        var pass = results.Count(r => r.Status == "PASS");
        var fail = results.Count(r => r.Status == "FAIL");
        var score = total == 0 ? 100 : (int)((double)pass / total * 100);

        var vpcRows = await _ec2Service.GetVpcResourceRowsAsync();
        var subnetRows = await _ec2Service.GetSubnetResourceRowsAsync();
        var routeTableRows = await _ec2Service.GetRouteTableResourceRowsAsync();
        var instanceRows = await _ec2Service.GetInstanceResourceRowsAsync();

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
            Vpcs = vpcRows,
            Subnets = subnetRows,
            RouteTables = routeTableRows,
            Instances = instanceRows
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

        var total = networkResults.Count;
        var pass = networkResults.Count(r => r.Status == "PASS");
        var fail = networkResults.Count(r => r.Status == "FAIL");
        var score = total == 0 ? 100 : (int)((double)pass / total * 100);

        var vpcRows = await _ec2Service.GetVpcResourceRowsAsync();
        var subnetRows = await _ec2Service.GetSubnetResourceRowsAsync();
        var routeTableRows = await _ec2Service.GetRouteTableResourceRowsAsync();
        var instanceRows = await _ec2Service.GetInstanceResourceRowsAsync();

        var report = new ValidationReport
        {
            Summary = new ValidationSummary
            {
                Total = total,
                Pass = pass,
                Fail = fail,
                Score = score
            },
            Results = networkResults,
            Vpcs = vpcRows,
            Subnets = subnetRows,
            RouteTables = routeTableRows,
            Instances = instanceRows
        };

        return Ok(new ApiResponse<ValidationReport>
        {
            Success = true,
            Data = report,
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpPost("compute")]
    public IActionResult ValidateCompute()
    {
        var results = _computeValidator.Run();

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
