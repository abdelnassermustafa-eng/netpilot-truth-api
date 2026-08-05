using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Networking;
using TruthApi.Services.Aws.Networking.Infrastructure;

namespace TruthApi.Services.Aws.Networking.Discoverers.Security;

/// <summary>
/// Discovers Security Groups and their complete ingress and egress rules
/// in one AWS Region.
/// </summary>
public sealed class SecurityGroupDiscoverer
{
    public async Task<IReadOnlyList<AwsSecurityGroupInfo>> DiscoverAsync(
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

        var results = new List<AwsSecurityGroupInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeSecurityGroupsAsync(
                new DescribeSecurityGroupsRequest
                {
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var group in response.SecurityGroups ?? [])
            {
                var tags = AwsTagHelper.ToDictionary(group.Tags);

                results.Add(new AwsSecurityGroupInfo
                {
                    GroupId = group.GroupId ?? "",
                    GroupName = group.GroupName ?? "",
                    Name = AwsTagHelper.GetName(tags),
                    Description = group.Description ?? "",
                    VpcId = group.VpcId ?? "",
                    OwnerId = group.OwnerId ?? "",
                    Region = region,
                    IngressRules = (group.IpPermissions ?? [])
                        .Select(ToRuleInfo)
                        .ToList(),
                    EgressRules = (group.IpPermissionsEgress ?? [])
                        .Select(ToRuleInfo)
                        .ToList(),
                    Tags = tags
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }

    private static AwsSecurityGroupRuleInfo ToRuleInfo(
        IpPermission permission)
    {
        var descriptions = new List<string>();

        descriptions.AddRange(
            (permission.Ipv4Ranges ?? [])
                .Select(range => range.Description)
                .Where(description =>
                    !string.IsNullOrWhiteSpace(description))!);

        descriptions.AddRange(
            (permission.Ipv6Ranges ?? [])
                .Select(range => range.Description)
                .Where(description =>
                    !string.IsNullOrWhiteSpace(description))!);

        descriptions.AddRange(
            (permission.UserIdGroupPairs ?? [])
                .Select(pair => pair.Description)
                .Where(description =>
                    !string.IsNullOrWhiteSpace(description))!);

        return new AwsSecurityGroupRuleInfo
        {
            Protocol = permission.IpProtocol ?? "",
            FromPort = permission.FromPort,
            ToPort = permission.ToPort,

            Ipv4Ranges = (permission.Ipv4Ranges ?? [])
                .Select(range => range.CidrIp ?? "")
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList(),

            Ipv6Ranges = (permission.Ipv6Ranges ?? [])
                .Select(range => range.CidrIpv6 ?? "")
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList(),

            PrefixListIds = (permission.PrefixListIds ?? [])
                .Select(prefix => prefix.Id ?? "")
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToList(),

            ReferencedSecurityGroupIds =
                (permission.UserIdGroupPairs ?? [])
                    .Select(pair => pair.GroupId ?? "")
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .ToList(),

            Description = string.Join(
                "; ",
                descriptions.Distinct())
        };
    }
}
