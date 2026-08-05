using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Networking;
using TruthApi.Services.Aws.Networking.Infrastructure;

namespace TruthApi.Services.Aws.Networking.Discoverers.Connectivity;

/// <summary>
/// Discovers Elastic Network Interfaces and their addresses, attachments,
/// Security Groups, tags, and ownership information in one AWS Region.
/// </summary>
public sealed class NetworkInterfaceDiscoverer
{
    public async Task<IReadOnlyList<AwsNetworkInterfaceInfo>> DiscoverAsync(
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

        var results = new List<AwsNetworkInterfaceInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeNetworkInterfacesAsync(
                new DescribeNetworkInterfacesRequest
                {
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var networkInterface in
                     response.NetworkInterfaces ?? [])
            {
                var tags = AwsTagHelper.ToDictionary(
                    networkInterface.TagSet);

                results.Add(new AwsNetworkInterfaceInfo
                {
                    NetworkInterfaceId =
                        networkInterface.NetworkInterfaceId ?? "",
                    Name = AwsTagHelper.GetName(tags),
                    Description =
                        networkInterface.Description ?? "",
                    InterfaceType =
                        networkInterface.InterfaceType?.Value ?? "",
                    Status =
                        networkInterface.Status?.Value ?? "",
                    VpcId =
                        networkInterface.VpcId ?? "",
                    SubnetId =
                        networkInterface.SubnetId ?? "",
                    AvailabilityZone =
                        networkInterface.AvailabilityZone ?? "",
                    OwnerId =
                        networkInterface.OwnerId ?? "",
                    RequesterId =
                        networkInterface.RequesterId ?? "",
                    RequesterManaged =
                        networkInterface.RequesterManaged ?? false,
                    SourceDestinationCheck =
                        networkInterface.SourceDestCheck ?? false,
                    MacAddress =
                        networkInterface.MacAddress ?? "",
                    PrivateIpAddress =
                        networkInterface.PrivateIpAddress ?? "",
                    PrivateDnsName =
                        networkInterface.PrivateDnsName ?? "",
                    Region = region,

                    SecurityGroupIds =
                        (networkInterface.Groups ?? [])
                            .Select(group => group.GroupId ?? "")
                            .Where(id =>
                                !string.IsNullOrWhiteSpace(id))
                            .ToList(),

                    Ipv6Addresses =
                        (networkInterface.Ipv6Addresses ?? [])
                            .Select(address =>
                                address.Ipv6Address ?? "")
                            .Where(address =>
                                !string.IsNullOrWhiteSpace(address))
                            .ToList(),

                    PrivateIpAddresses =
                        (networkInterface.PrivateIpAddresses ?? [])
                            .Select(ToPrivateIpInfo)
                            .ToList(),

                    Attachment = ToAttachmentInfo(
                        networkInterface.Attachment),

                    Tags = tags
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }

    private static AwsNetworkInterfacePrivateIpInfo ToPrivateIpInfo(
        NetworkInterfacePrivateIpAddress privateIp)
    {
        return new AwsNetworkInterfacePrivateIpInfo
        {
            PrivateIpAddress =
                privateIp.PrivateIpAddress ?? "",
            IsPrimary =
                privateIp.Primary ?? false,
            PrivateDnsName =
                privateIp.PrivateDnsName ?? "",
            PublicIp =
                privateIp.Association?.PublicIp ?? "",
            PublicDnsName =
                privateIp.Association?.PublicDnsName ?? "",
            AllocationId =
                privateIp.Association?.AllocationId ?? "",
            AssociationId =
                privateIp.Association?.AssociationId ?? ""
        };
    }

    private static AwsNetworkInterfaceAttachmentInfo? ToAttachmentInfo(
        NetworkInterfaceAttachment? attachment)
    {
        if (attachment is null)
        {
            return null;
        }

        return new AwsNetworkInterfaceAttachmentInfo
        {
            AttachmentId =
                attachment.AttachmentId ?? "",
            InstanceId =
                attachment.InstanceId ?? "",
            InstanceOwnerId =
                attachment.InstanceOwnerId ?? "",
            DeviceIndex =
                attachment.DeviceIndex ?? 0,
            NetworkCardIndex =
                attachment.NetworkCardIndex ?? 0,
            Status =
                attachment.Status?.Value ?? "",
            DeleteOnTermination =
                attachment.DeleteOnTermination ?? false,
            AttachTime =
                attachment.AttachTime
        };
    }
}
