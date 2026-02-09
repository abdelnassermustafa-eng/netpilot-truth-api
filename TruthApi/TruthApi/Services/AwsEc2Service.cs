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

        public AwsEc2Service(IOptions<AwsConfig> awsConfig)
        {
            var region = RegionEndpoint.GetBySystemName(
                awsConfig.Value.Region);

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
    }
}
