using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Networking;
using TruthApi.Services.Aws.Networking.Infrastructure;

namespace TruthApi.Services.Aws.Networking.Discoverers.Connectivity;

/// <summary>
/// Discovers Elastic IP addresses in one AWS Region.
/// </summary>
public sealed class ElasticIpDiscoverer
{
    public async Task<IReadOnlyList<AwsElasticIpInfo>> DiscoverAsync(
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

        var response = await client.DescribeAddressesAsync(
            new DescribeAddressesRequest(),
            cancellationToken);

        return (response.Addresses ?? [])
            .Select(address =>
            {
                var tags = AwsTagHelper.ToDictionary(address.Tags);

                return new AwsElasticIpInfo
                {
                    AllocationId = address.AllocationId ?? "",
                    AssociationId = address.AssociationId ?? "",
                    PublicIp = address.PublicIp ?? "",
                    PrivateIpAddress =
                        address.PrivateIpAddress ?? "",
                    NetworkInterfaceId =
                        address.NetworkInterfaceId ?? "",
                    NetworkInterfaceOwnerId =
                        address.NetworkInterfaceOwnerId ?? "",
                    InstanceId = address.InstanceId ?? "",
                    Domain = address.Domain?.Value ?? "",
                    NetworkBorderGroup =
                        address.NetworkBorderGroup ?? "",
                    PublicIpv4Pool =
                        address.PublicIpv4Pool ?? "",
                    CustomerOwnedIp =
                        address.CustomerOwnedIp ?? "",
                    CustomerOwnedIpv4Pool =
                        address.CustomerOwnedIpv4Pool ?? "",
                    Name = AwsTagHelper.GetName(tags),
                    Region = region,
                    Tags = tags
                };
            })
            .ToList();
    }
}
