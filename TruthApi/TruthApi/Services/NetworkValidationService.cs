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

        // ================================
        // Rule 1: VPC must have subnets
        // ================================
        foreach (var vpc in vpcs)
        {
            var vpcSubnets = subnets
                .Where(s => s.VpcId == vpc.VpcId)
                .ToList();

            if (vpcSubnets.Count > 0)
            {
                results.Add(new ValidationResult
                {
                    Rule = "VPC_HAS_SUBNET",
                    ResourceId = vpc.VpcId,
                    Status = "PASS",
                    Severity = "INFO",
                    Message = "VPC contains at least one subnet"
                });
            }
            else
            {
                results.Add(new ValidationResult
                {
                    Rule = "VPC_HAS_SUBNET",
                    ResourceId = vpc.VpcId,
                    Status = "FAIL",
                    Severity = "CRITICAL",
                    Message = "VPC has no subnets"
                });
            }
        }

        // ====================================
        // Rule 2: VPC must have route table
        // ====================================
        foreach (var vpc in vpcs)
        {
            var vpcRouteTables = routeTables
                .Where(r => r.VpcId == vpc.VpcId)
                .ToList();

            if (vpcRouteTables.Count > 0)
            {
                results.Add(new ValidationResult
                {
                    Rule = "VPC_HAS_ROUTE_TABLE",
                    ResourceId = vpc.VpcId,
                    Status = "PASS",
                    Severity = "INFO",
                    Message = "VPC has at least one route table"
                });
            }
            else
            {
                results.Add(new ValidationResult
                {
                    Rule = "VPC_HAS_ROUTE_TABLE",
                    ResourceId = vpc.VpcId,
                    Status = "FAIL",
                    Severity = "CRITICAL",
                    Message = "VPC has no route tables"
                });
            }
        }

        // =========================================
        // Rule 3: Subnet must belong to a VPC
        // =========================================
        foreach (var subnet in subnets)
        {
            var parentVpc = vpcs.FirstOrDefault(v => v.VpcId == subnet.VpcId);

            if (parentVpc != null)
            {
                results.Add(new ValidationResult
                {
                    Rule = "SUBNET_HAS_PARENT_VPC",
                    ResourceId = subnet.SubnetId,
                    Status = "PASS",
                    Severity = "INFO",
                    Message = "Subnet belongs to a VPC"
                });
            }
            else
            {
                results.Add(new ValidationResult
                {
                    Rule = "SUBNET_HAS_PARENT_VPC",
                    ResourceId = subnet.SubnetId,
                    Status = "FAIL",
                    Severity = "CRITICAL",
                    Message = "Subnet is not associated with any VPC"
                });
            }
        }

        return results;
    }

}

