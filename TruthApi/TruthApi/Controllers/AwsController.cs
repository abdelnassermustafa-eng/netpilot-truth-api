using Amazon.EC2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruthApi.Services;
using TruthApi.Models;

namespace TruthApi.Controllers
{
    [ApiController]
    [Route("api/v1/aws")]
    [Authorize(Roles = "Admin")]
    public class AwsController : ControllerBase
    {
        private readonly AwsEc2Service _ec2Service;

        public AwsController(AwsEc2Service ec2Service)
        {
            _ec2Service = ec2Service;
        }


        [HttpGet("vpcs")]
        public async Task<IActionResult> GetVpcs()
        {
            try
            {
                var vpcs = await _ec2Service.GetVpcsAsync();

                // Ensure we never operate on null
                if (vpcs == null)
                    vpcs = new List<Amazon.EC2.Model.Vpc>();

                var result = vpcs.Select(v => new
                {
                    vpcId = v.VpcId,
                    cidr = v.CidrBlock,
                    state = v.State?.Value,
                    isDefault = v.IsDefault
                });

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AWS ERROR: {ex.Message}");

                return Ok(new ApiResponse<object>
                {
                    Success = false,
                    Data = null
                });
            }
        }

        [HttpGet("subnets")]
        [Authorize(Roles = "Admin,Viewer")]
        public async Task<IActionResult> GetSubnets()
        {
            var subnets = await _ec2Service.GetSubnetsAsync();

            var result = subnets.Select(s => new SubnetDto
            {
                SubnetId = s.SubnetId,
                VpcId = s.VpcId,
                Cidr = s.CidrBlock,
                AvailabilityZone = s.AvailabilityZone,
                State = s.State.Value
            }).ToList();

            return Ok(new ApiResponse<List<SubnetDto>>
            {
                Success = true,
                Data = result,
                Timestamp = DateTime.UtcNow
            });
        }


        [HttpGet("route-tables")]
        [Authorize(Roles = "Admin,Viewer")]
        public async Task<IActionResult> GetRouteTables()
        {
            var routeTables = await _ec2Service.GetRouteTablesAsync();

            var result = routeTables.Select(r => new RouteTableDto
            {
                RouteTableId = r.RouteTableId,
                VpcId = r.VpcId,
                RouteCount = r.Routes.Count
            }).ToList();

            return Ok(new ApiResponse<List<RouteTableDto>>
            {
                Success = true,
                Data = result,
                Timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("instances")]
        [Authorize(Roles = "Admin,Viewer")]
        public async Task<IActionResult> GetInstances()
        {
            var instances = await _ec2Service.GetInstancesAsync();

            var result = instances.Select(i => new
            {
                instanceId = i.InstanceId,
                instanceType = i.InstanceType?.Value,
                state = i.State?.Name?.Value,
                subnetId = i.SubnetId,
                vpcId = i.VpcId,
                availabilityZone = i.Placement?.AvailabilityZone
            }).ToList();

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = result,
                Timestamp = DateTime.UtcNow
            });
        }


    }
}

