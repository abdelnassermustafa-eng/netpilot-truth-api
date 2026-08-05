using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Networking;
using TruthApi.Services.Aws.Networking.Infrastructure;

namespace TruthApi.Services.Aws.Networking.Discoverers.Security;

/// <summary>
/// Discovers Network ACLs, subnet associations, and ordered entries
/// in one AWS Region.
/// </summary>
public sealed class NetworkAclDiscoverer
{
    public async Task<IReadOnlyList<AwsNetworkAclInfo>> DiscoverAsync(
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

        var results = new List<AwsNetworkAclInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeNetworkAclsAsync(
                new DescribeNetworkAclsRequest
                {
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var acl in response.NetworkAcls ?? [])
            {
                var tags = AwsTagHelper.ToDictionary(acl.Tags);

                results.Add(new AwsNetworkAclInfo
                {
                    NetworkAclId = acl.NetworkAclId ?? "",
                    Name = AwsTagHelper.GetName(tags),
                    VpcId = acl.VpcId ?? "",
                    OwnerId = acl.OwnerId ?? "",
                    IsDefault = acl.IsDefault ?? false,
                    Region = region,
                    Associations = (acl.Associations ?? [])
                        .Select(ToAssociationInfo)
                        .OrderBy(association => association.SubnetId)
                        .ToList(),
                    Entries = (acl.Entries ?? [])
                        .Select(ToEntryInfo)
                        .OrderBy(entry => entry.IsEgress)
                        .ThenBy(entry => entry.RuleNumber)
                        .ToList(),
                    Tags = tags
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }

    private static AwsNetworkAclAssociationInfo ToAssociationInfo(
        NetworkAclAssociation association)
    {
        return new AwsNetworkAclAssociationInfo
        {
            AssociationId =
                association.NetworkAclAssociationId ?? "",
            NetworkAclId = association.NetworkAclId ?? "",
            SubnetId = association.SubnetId ?? ""
        };
    }

    private static AwsNetworkAclEntryInfo ToEntryInfo(
        NetworkAclEntry entry)
    {
        return new AwsNetworkAclEntryInfo
        {
            RuleNumber = entry.RuleNumber ?? 0,
            IsEgress = entry.Egress ?? false,
            Protocol = entry.Protocol ?? "",
            RuleAction = entry.RuleAction?.Value ?? "",
            CidrBlock = entry.CidrBlock ?? "",
            Ipv6CidrBlock = entry.Ipv6CidrBlock ?? "",
            FromPort = entry.PortRange?.From,
            ToPort = entry.PortRange?.To,
            IcmpType = entry.IcmpTypeCode?.Type,
            IcmpCode = entry.IcmpTypeCode?.Code
        };
    }
}
