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

        // expose region
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
        // EC2 Instances
        // ===============================
        public async Task<List<Instance>> GetInstancesAsync()
        {
            var response = await _ec2Client.DescribeInstancesAsync(
                new DescribeInstancesRequest());

            var instances = new List<Instance>();

            if (response.Reservations != null)
            {
                foreach (var res in response.Reservations)
                {
                    if (res.Instances != null)
                        instances.AddRange(res.Instances);
                }
            }

            return instances;
        }

        // ===============================
        // Resource Row Helpers
        // ===============================
        public async Task<List<VpcResourceRow>> GetVpcResourceRowsAsync()
        {
            var vpcs = await GetVpcsAsync();
            var rows = new List<VpcResourceRow>();

            foreach (var v in vpcs)
            {
                rows.Add(new VpcResourceRow
                {
                    VpcId = v.VpcId ?? "",
                    Cidr = v.CidrBlock ?? "",
                    State = v.State?.Value ?? "",
                    Region = Region
                });
            }

            return rows;
        }

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
                    Region = Region
                });
            }

            return rows;
        }

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

        public async Task<List<InstanceResourceRow>> GetInstanceResourceRowsAsync()
        {
            var instances = await GetInstancesAsync();
            var rows = new List<InstanceResourceRow>();

            foreach (var i in instances)
            {
                rows.Add(new InstanceResourceRow
                {
                    InstanceId = i.InstanceId ?? "",
                    InstanceType = i.InstanceType?.Value ?? "",
                    State = i.State?.Name?.Value ?? "",
                    SubnetId = i.SubnetId ?? "",
                    VpcId = i.VpcId ?? "",
                    AvailabilityZone = i.Placement?.AvailabilityZone ?? "",
                    Region = Region
                });
            }

            return rows;
        }

        // ===============================
        // Internet Gateways
        // ===============================
        public async Task<List<InternetGateway>> GetInternetGatewaysAsync()
        {
            var response = await _ec2Client.DescribeInternetGatewaysAsync(
                new DescribeInternetGatewaysRequest());

            return response.InternetGateways ?? new List<InternetGateway>();
        }

        public async Task<List<InternetGatewayResourceRow>> GetInternetGatewayResourceRowsAsync()
        {
            var igws = await GetInternetGatewaysAsync();
            var rows = new List<InternetGatewayResourceRow>();

            foreach (var igw in igws)
            {
                var attachedVpcs = igw.Attachments
                    .Where(a => !string.IsNullOrEmpty(a.VpcId))
                    .Select(a => a.VpcId);

                var state = igw.Attachments.FirstOrDefault()?.State?.Value ?? "detached";

                rows.Add(new InternetGatewayResourceRow
                {
                    IgwId = igw.InternetGatewayId ?? "",
                    AttachedVpcIds = string.Join(",", attachedVpcs),
                    AttachmentState = state,
                    Region = Region
                });
            }

            return rows;
        }
    }
}
