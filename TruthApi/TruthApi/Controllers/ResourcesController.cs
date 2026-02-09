using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruthApi.Services;

namespace TruthApi.Controllers;

[ApiController]
[Route("api/v1/resources")]
[Authorize(Roles = "Admin,Viewer")]
public class ResourcesController : ControllerBase
{
    private readonly AwsEc2Service _ec2Service;

    public ResourcesController(AwsEc2Service ec2Service)
    {
        _ec2Service = ec2Service;
    }

    // ===============================
    // VPC Resources
    // ===============================
    [HttpGet("vpcs")]
    public async Task<IActionResult> GetVpcs()
    {
        var rows = await _ec2Service.GetVpcResourceRowsAsync();
        return Ok(rows);
    }
}
