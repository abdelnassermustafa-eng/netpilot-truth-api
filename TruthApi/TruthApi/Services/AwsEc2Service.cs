using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Microsoft.Extensions.Options;
using TruthApi.Models;

namespace TruthApi.Services
{
    public class AwsEc2Service
    {
        private readonly IAmazonEC2 _ec2Client;

        // Phase 6.2 — expose region for validation results
        public string Region { get; }

        public AwsEc2Service(IOptions<AwsConfig> awsConfig)
        {
            Region = awsConfig.Value.Region;

            var region = RegionEndpoint.GetBySystemName(Region);
            _ec2Client = new AmazonEC2Client(region);
        }

        // ===============================
        // VPCs
        // ===============================
        public async Task<List<Vpc>> GetVpcsAsync()
        {
            var response = await _ec2Client.DescribeVpcsAsync(
                new DescribeVpcsRequest());

            return response.Vpcs ?? new List<Vpc>();
        }

        // ===============================
        // Subnets
        // ===============================
        public async Task<List<Subnet>> GetSubnetsAsync()
        {
            var response = await _ec2Client.DescribeSubnetsAsync(
                new DescribeSubnetsRequest());

            return response.Subnets ?? new List<Subnet>();
        }

        // ===============================
        // Route Tables
        // ===============================
        public async Task<List<RouteTable>> GetRouteTablesAsync()
        {
            var response = await _ec2Client.DescribeRouteTablesAsync(
                new DescribeRouteTablesRequest());

            return response.RouteTables ?? new List<RouteTable>();
        }

        // ===============================
        // VPC Resource Rows (Phase 6.2)
        // ===============================
        public async Task<List<VpcResourceRow>> GetVpcResourceRowsAsync()
        {
            var vpcs = await GetVpcsAsync();
            var rows = new List<VpcResourceRow>();

            foreach (var vpc in vpcs)
            {
                rows.Add(new VpcResourceRow
                {
                    VpcId = vpc.VpcId ?? "",
                    Cidr = vpc.CidrBlock ?? "",
                    State = vpc.State?.Value ?? "",
                    Region = Region
                });
            }

            return rows;
        }

        // ===============================
        // Subnet Resource Rows (Phase 6.2)
        // ===============================
        public async Task<List<SubnetResourceRow>> GetSubnetResourceRowsAsync()
        {
            var subnets = await GetSubnetsAsync();
            var rows = new List<SubnetResourceRow>();

            foreach (var s in subnets)
            {
                rows.Add(new SubnetResourceRow
                {
                    SubnetId = s.SubnetId ?? "",
                    VpcId = s.VpcId ?? "",
                    Cidr = s.CidrBlock ?? "",
                    AvailabilityZone = s.AvailabilityZone ?? "",
                    State = s.State?.Value ?? "",
                    Action = s.DefaultForAz == true ? "Cannot be deleted" : "Can be deleted",
                    Region = Region
                });
            }

            return rows;
        }

        // ===============================
        // Route Table Resource Rows (Phase 6.2)
        // ===============================
        public async Task<List<RouteTableResourceRow>> GetRouteTableResourceRowsAsync()
        {
            var routeTables = await GetRouteTablesAsync();
            var rows = new List<RouteTableResourceRow>();

            foreach (var rt in routeTables)
            {
                bool isMain = rt.Associations.Any(a => a.Main == true);

                var associatedSubnet = rt.Associations
                    .FirstOrDefault(a => a.Main != true && !string.IsNullOrEmpty(a.SubnetId))
                    ?.SubnetId ?? "";

                var routeCidr = rt.Routes
                    .FirstOrDefault(r => !string.IsNullOrEmpty(r.DestinationCidrBlock))
                    ?.DestinationCidrBlock ?? "";

                rows.Add(new RouteTableResourceRow
                {
                    RouteTableId = rt.RouteTableId ?? "",
                    VpcId = rt.VpcId ?? "",
                    IsMain = isMain,
                    AssociationType = isMain ? "Main" : "Non-main",
                    AssociatedSubnetId = associatedSubnet,
                    DestinationCidr = routeCidr,
                    Region = Region
                });
            }

            return rows;
        }
    }
}
