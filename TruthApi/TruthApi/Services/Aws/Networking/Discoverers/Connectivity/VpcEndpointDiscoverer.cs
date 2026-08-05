using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Networking;
using TruthApi.Services.Aws.Networking.Infrastructure;

namespace TruthApi.Services.Aws.Networking.Discoverers.Connectivity;

/// <summary>
/// Discovers VPC Endpoints and their related route tables, subnets,
/// network interfaces, security groups, DNS entries, and policy documents
/// in one AWS Region.
/// </summary>
public sealed class VpcEndpointDiscoverer
{
    public async Task<IReadOnlyList<AwsVpcEndpointInfo>> DiscoverAsync(
        IAmazonEC2 client,
        string region,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);

        if (string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException(
                "AWS Region is required.",
                nameof(region));
        }

        var results = new List<AwsVpcEndpointInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeVpcEndpointsAsync(
                new DescribeVpcEndpointsRequest
                {
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var endpoint in response.VpcEndpoints ?? [])
            {
                var tags = AwsTagHelper.ToDictionary(endpoint.Tags);

                results.Add(new AwsVpcEndpointInfo
                {
                    VpcEndpointId = endpoint.VpcEndpointId ?? "",
                    Name = AwsTagHelper.GetName(tags),
                    VpcId = endpoint.VpcId ?? "",
                    ServiceName = endpoint.ServiceName ?? "",
                    EndpointType =
                        endpoint.VpcEndpointType?.Value ?? "",
                    State = endpoint.State?.Value ?? "",
                    OwnerId = endpoint.OwnerId ?? "",
                    PrivateDnsEnabled =
                        endpoint.PrivateDnsEnabled ?? false,
                    CreatedAt = endpoint.CreationTimestamp,
                    PolicyDocument = endpoint.PolicyDocument ?? "",
                    Region = region,

                    RouteTableIds = (endpoint.RouteTableIds ?? [])
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .ToList(),

                    SubnetIds = (endpoint.SubnetIds ?? [])
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .ToList(),

                    NetworkInterfaceIds =
                        (endpoint.NetworkInterfaceIds ?? [])
                            .Where(id =>
                                !string.IsNullOrWhiteSpace(id))
                            .ToList(),

                    SecurityGroups = (endpoint.Groups ?? [])
                        .Select(group =>
                            new AwsVpcEndpointSecurityGroupInfo
                            {
                                GroupId = group.GroupId ?? "",
                                GroupName = group.GroupName ?? ""
                            })
                        .ToList(),

                    DnsEntries = (endpoint.DnsEntries ?? [])
                        .Select(entry =>
                            new AwsVpcEndpointDnsEntryInfo
                            {
                                DnsName = entry.DnsName ?? "",
                                HostedZoneId =
                                    entry.HostedZoneId ?? ""
                            })
                        .ToList(),

                    Tags = tags
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }
}
