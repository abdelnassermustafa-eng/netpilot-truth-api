using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Networking;
using TruthApi.Services.Aws.Networking.Infrastructure;

namespace TruthApi.Services.Aws.Networking.Discoverers.Connectivity;

/// <summary>
/// Discovers all NAT Gateways in one AWS Region.
/// </summary>
public sealed class NatGatewayDiscoverer
{
    public async Task<IReadOnlyList<AwsNatGatewayInfo>> DiscoverAsync(
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

        var results = new List<AwsNatGatewayInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeNatGatewaysAsync(
                new DescribeNatGatewaysRequest
                {
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var gateway in response.NatGateways ?? [])
            {
                var tags = AwsTagHelper.ToDictionary(gateway.Tags);

                results.Add(new AwsNatGatewayInfo
                {
                    NatGatewayId = gateway.NatGatewayId ?? "",
                    Name = AwsTagHelper.GetName(tags),
                    VpcId = gateway.VpcId ?? "",
                    SubnetId = gateway.SubnetId ?? "",
                    State = gateway.State?.Value ?? "",
                    ConnectivityType =
                        gateway.ConnectivityType?.Value ?? "",
                    FailureCode = gateway.FailureCode ?? "",
                    FailureMessage = gateway.FailureMessage ?? "",
                    CreatedAt = gateway.CreateTime,
                    DeletedAt = gateway.DeleteTime,
                    Region = region,
                    Addresses = (gateway.NatGatewayAddresses ?? [])
                        .Select(address =>
                            new AwsNatGatewayAddressInfo
                            {
                                AllocationId =
                                    address.AllocationId ?? "",
                                AssociationId =
                                    address.AssociationId ?? "",
                                NetworkInterfaceId =
                                    address.NetworkInterfaceId ?? "",
                                PrivateIp = address.PrivateIp ?? "",
                                PublicIp = address.PublicIp ?? "",
                                IsPrimary = address.IsPrimary ?? false,
                                Status = address.Status?.Value ?? ""
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
