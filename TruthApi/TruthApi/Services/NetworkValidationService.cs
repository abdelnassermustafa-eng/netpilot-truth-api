using TruthApi.Models;

namespace TruthApi.Services;

public class NetworkValidationService
{
    private readonly AwsEc2Service _ec2Service;

    public NetworkValidationService(AwsEc2Service ec2Service)
    {
        _ec2Service = ec2Service;
    }

    public async Task<List<ValidationResult>> ValidateNetworkAsync()
    {
        var results = new List<ValidationResult>();

        var vpcs = await _ec2Service.GetVpcsAsync();
        var subnets = await _ec2Service.GetSubnetsAsync();
        var routeTables = await _ec2Service.GetRouteTablesAsync();

        var region = _ec2Service.Region;

        // ================================
        // Rule 1: VPC must have subnets
        // ================================
        foreach (var vpc in vpcs)
        {
            var vpcSubnets = subnets
                .Where(s => s.VpcId == vpc.VpcId)
                .ToList();

            bool pass = vpcSubnets.Count > 0;

            results.Add(new ValidationResult
            {
                Category = "Networking",
                Rule = "VPC_HAS_SUBNET",
                ResourceType = "VPC",
                ResourceId = vpc.VpcId,
                Region = region,
                CidrOrDestination = vpc.CidrBlock,
                RouteTableId = "",
                Status = pass ? "PASS" : "FAIL",
                Severity = pass ? "INFO" : "CRITICAL",
                Message = pass
                    ? "VPC contains at least one subnet"
                    : "VPC has no subnets",
                Action = pass
                    ? "No action required"
                    : "Create at least one subnet in this VPC"
            });
        }

        // ====================================
        // Rule 2: VPC must have route table
        // ====================================
        foreach (var vpc in vpcs)
        {
            var vpcRouteTables = routeTables
                .Where(r => r.VpcId == vpc.VpcId)
                .ToList();

            bool pass = vpcRouteTables.Count > 0;
            var routeTableId = pass ? vpcRouteTables.First().RouteTableId : "";

            results.Add(new ValidationResult
            {
                Category = "Networking",
                Rule = "VPC_HAS_ROUTE_TABLE",
                ResourceType = "VPC",
                ResourceId = vpc.VpcId,
                Region = region,
                CidrOrDestination = vpc.CidrBlock,
                RouteTableId = routeTableId,
                Status = pass ? "PASS" : "FAIL",
                Severity = pass ? "INFO" : "CRITICAL",
                Message = pass
                    ? "VPC has at least one route table"
                    : "VPC has no route tables",
                Action = pass
                    ? "No action required"
                    : "Create or associate a route table"
            });
        }

        // =========================================
        // Rule 3: Subnet must belong to a VPC
        // =========================================
        foreach (var subnet in subnets)
        {
            var parentVpc = vpcs.FirstOrDefault(v => v.VpcId == subnet.VpcId);
            bool pass = parentVpc != null;

            results.Add(new ValidationResult
            {
                Category = "Networking",
                Rule = "SUBNET_HAS_PARENT_VPC",
                ResourceType = "Subnet",
                ResourceId = subnet.SubnetId,
                Region = region,
                CidrOrDestination = subnet.CidrBlock,
                RouteTableId = "",
                Status = pass ? "PASS" : "FAIL",
                Severity = pass ? "INFO" : "CRITICAL",
                Message = pass
                    ? "Subnet belongs to a VPC"
                    : "Subnet is not associated with any VPC",
                Action = pass
                    ? "No action required"
                    : "Associate subnet with a valid VPC"
            });
        }

        return results;
    }
}
