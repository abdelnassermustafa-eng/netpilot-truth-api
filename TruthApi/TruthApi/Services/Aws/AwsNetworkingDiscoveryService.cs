using Amazon.EC2.Model;
using Ec2Route = Amazon.EC2.Model.Route;
using TruthApi.Models.Aws.Networking;

namespace TruthApi.Services.Aws;

/// <summary>
/// Discovers live AWS networking resources across one or more Regions.
///
/// This service returns strongly typed domain models. Conversion into the
/// universal WorkbenchItem envelope will be performed by a separate adapter.
/// </summary>
public sealed class AwsNetworkingDiscoveryService
{
    private readonly AwsClientFactory _clientFactory;
    private readonly AwsIdentityService _identityService;

    public AwsNetworkingDiscoveryService(
        AwsClientFactory clientFactory,
        AwsIdentityService identityService)
    {
        _clientFactory = clientFactory;
        _identityService = identityService;
    }

    public async Task<AwsNetworkingInventory> DiscoverAsync(
        IReadOnlyCollection<string>? requestedRegions = null,
        CancellationToken cancellationToken = default)
    {
        var identity = await _identityService.GetCurrentIdentityAsync(
            cancellationToken);

        var regions = NormalizeRegions(requestedRegions);

        var discoveryTasks = regions
            .Select(region => DiscoverRegionAsync(region, cancellationToken))
            .ToArray();

        var regionalResults = await Task.WhenAll(discoveryTasks);

        return new AwsNetworkingInventory
        {
            AccountId = identity.AccountId,
            Regions = regions,
            Vpcs = regionalResults
                .SelectMany(result => result.Vpcs)
                .OrderBy(vpc => vpc.Region)
                .ThenBy(vpc => vpc.Name)
                .ThenBy(vpc => vpc.VpcId)
                .ToList(),
            Subnets = regionalResults
                .SelectMany(result => result.Subnets)
                .OrderBy(subnet => subnet.Region)
                .ThenBy(subnet => subnet.AvailabilityZone)
                .ThenBy(subnet => subnet.Name)
                .ThenBy(subnet => subnet.SubnetId)
                .ToList(),
            RouteTables = regionalResults
                .SelectMany(result => result.RouteTables)
                .OrderBy(routeTable => routeTable.Region)
                .ThenBy(routeTable => routeTable.Name)
                .ThenBy(routeTable => routeTable.RouteTableId)
                .ToList(),
            InternetGateways = regionalResults
                .SelectMany(result => result.InternetGateways)
                .OrderBy(gateway => gateway.Region)
                .ThenBy(gateway => gateway.Name)
                .ThenBy(gateway => gateway.InternetGatewayId)
                .ToList(),
            NatGateways = regionalResults
                .SelectMany(result => result.NatGateways)
                .OrderBy(gateway => gateway.Region)
                .ThenBy(gateway => gateway.Name)
                .ThenBy(gateway => gateway.NatGatewayId)
                .ToList(),
            SecurityGroups = regionalResults
                .SelectMany(result => result.SecurityGroups)
                .OrderBy(group => group.Region)
                .ThenBy(group => group.Name)
                .ThenBy(group => group.GroupName)
                .ThenBy(group => group.GroupId)
                .ToList(),
            NetworkAcls = regionalResults
                .SelectMany(result => result.NetworkAcls)
                .OrderBy(acl => acl.Region)
                .ThenBy(acl => acl.Name)
                .ThenBy(acl => acl.NetworkAclId)
                .ToList(),
            VpcEndpoints = regionalResults
                .SelectMany(result => result.VpcEndpoints)
                .OrderBy(endpoint => endpoint.Region)
                .ThenBy(endpoint => endpoint.Name)
                .ThenBy(endpoint => endpoint.VpcEndpointId)
                .ToList(),
            ElasticIps = regionalResults
                .SelectMany(result => result.ElasticIps)
                .OrderBy(address => address.Region)
                .ThenBy(address => address.Name)
                .ThenBy(address => address.PublicIp)
                .ThenBy(address => address.AllocationId)
                .ToList(),
            NetworkInterfaces = regionalResults
                .SelectMany(result => result.NetworkInterfaces)
                .OrderBy(networkInterface => networkInterface.Region)
                .ThenBy(networkInterface => networkInterface.Name)
                .ThenBy(networkInterface =>
                    networkInterface.NetworkInterfaceId)
                .ToList(),
            Warnings = regionalResults
                .SelectMany(result => result.Warnings)
                .ToList(),
            DiscoveredAt = DateTimeOffset.UtcNow
        };
    }

    private IReadOnlyList<string> NormalizeRegions(
        IReadOnlyCollection<string>? requestedRegions)
    {
        var regions = requestedRegions?
            .Where(region => !string.IsNullOrWhiteSpace(region))
            .Select(region => region.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(region => region)
            .ToList();

        if (regions is { Count: > 0 })
        {
            return regions;
        }

        return new[] { _clientFactory.DefaultRegion };
    }

    private async Task<RegionalNetworkingResult> DiscoverRegionAsync(
        string region,
        CancellationToken cancellationToken)
    {
        try
        {
            var client = _clientFactory.GetEc2Client(region);

            var vpcsTask = GetAllVpcsAsync(client, region, cancellationToken);
            var subnetsTask =
                GetAllSubnetsAsync(client, region, cancellationToken);
            var routeTablesTask =
                GetAllRouteTablesAsync(client, region, cancellationToken);

            var internetGatewaysTask =
                GetAllInternetGatewaysAsync(
                    client,
                    region,
                    cancellationToken);

            var natGatewaysTask =
                GetAllNatGatewaysAsync(
                    client,
                    region,
                    cancellationToken);

            var securityGroupsTask =
                GetAllSecurityGroupsAsync(
                    client,
                    region,
                    cancellationToken);

            var networkAclsTask =
                GetAllNetworkAclsAsync(
                    client,
                    region,
                    cancellationToken);

            var vpcEndpointsTask =
                GetAllVpcEndpointsAsync(
                    client,
                    region,
                    cancellationToken);

            var elasticIpsTask =
                GetAllElasticIpsAsync(
                    client,
                    region,
                    cancellationToken);

            var networkInterfacesTask =
                GetAllNetworkInterfacesAsync(
                    client,
                    region,
                    cancellationToken);

            await Task.WhenAll(
                vpcsTask,
                subnetsTask,
                routeTablesTask,
                internetGatewaysTask,
                natGatewaysTask,
                securityGroupsTask,
                networkAclsTask,
                vpcEndpointsTask,
                elasticIpsTask,
                networkInterfacesTask);

            return new RegionalNetworkingResult
            {
                Vpcs = await vpcsTask,
                Subnets = await subnetsTask,
                RouteTables = await routeTablesTask,
                InternetGateways = await internetGatewaysTask,
                NatGateways = await natGatewaysTask,
                SecurityGroups = await securityGroupsTask,
                NetworkAcls = await networkAclsTask,
                VpcEndpoints = await vpcEndpointsTask,
                ElasticIps = await elasticIpsTask,
                NetworkInterfaces = await networkInterfacesTask
            };
        }
        catch (Exception exception)
        {
            return new RegionalNetworkingResult
            {
                Warnings =
                [
                    $"Region {region} could not be discovered: " +
                    exception.Message
                ]
            };
        }
    }

    private static async Task<IReadOnlyList<AwsVpcInfo>> GetAllVpcsAsync(
        Amazon.EC2.IAmazonEC2 client,
        string region,
        CancellationToken cancellationToken)
    {
        var results = new List<AwsVpcInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeVpcsAsync(
                new DescribeVpcsRequest
                {
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var vpc in response.Vpcs ?? [])
            {
                var tags = ToTagDictionary(vpc.Tags);

                results.Add(new AwsVpcInfo
                {
                    VpcId = vpc.VpcId ?? "",
                    Name = GetName(tags),
                    CidrBlock = vpc.CidrBlock ?? "",
                    State = vpc.State?.Value ?? "",
                    IsDefault = vpc.IsDefault ?? false,
                    Region = region,
                    Tags = tags
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }

    private static async Task<IReadOnlyList<AwsSubnetInfo>>
        GetAllSubnetsAsync(
            Amazon.EC2.IAmazonEC2 client,
            string region,
            CancellationToken cancellationToken)
    {
        var results = new List<AwsSubnetInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeSubnetsAsync(
                new DescribeSubnetsRequest
                {
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var subnet in response.Subnets ?? [])
            {
                var tags = ToTagDictionary(subnet.Tags);

                results.Add(new AwsSubnetInfo
                {
                    SubnetId = subnet.SubnetId ?? "",
                    Name = GetName(tags),
                    VpcId = subnet.VpcId ?? "",
                    CidrBlock = subnet.CidrBlock ?? "",
                    AvailabilityZone = subnet.AvailabilityZone ?? "",
                    AvailabilityZoneId = subnet.AvailabilityZoneId ?? "",
                    State = subnet.State?.Value ?? "",
                    MapPublicIpOnLaunch =
                        subnet.MapPublicIpOnLaunch ?? false,
                    AvailableIpAddressCount =
                        subnet.AvailableIpAddressCount ?? 0,
                    Region = region,
                    Tags = tags
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }

    private static async Task<IReadOnlyList<AwsRouteTableInfo>>
        GetAllRouteTablesAsync(
            Amazon.EC2.IAmazonEC2 client,
            string region,
            CancellationToken cancellationToken)
    {
        var results = new List<AwsRouteTableInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeRouteTablesAsync(
                new DescribeRouteTablesRequest
                {
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var routeTable in response.RouteTables ?? [])
            {
                var tags = ToTagDictionary(routeTable.Tags);
                var associations = routeTable.Associations ?? [];
                var routes = routeTable.Routes ?? [];

                results.Add(new AwsRouteTableInfo
                {
                    RouteTableId = routeTable.RouteTableId ?? "",
                    Name = GetName(tags),
                    VpcId = routeTable.VpcId ?? "",
                    IsMain = associations.Any(
                        association => association.Main == true),
                    Region = region,
                    Associations = associations
                        .Select(ToAssociationInfo)
                        .ToList(),
                    Routes = routes
                        .Select(ToRouteInfo)
                        .ToList(),
                    Tags = tags
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }

    private static async Task<IReadOnlyList<AwsInternetGatewayInfo>>
        GetAllInternetGatewaysAsync(
            Amazon.EC2.IAmazonEC2 client,
            string region,
            CancellationToken cancellationToken)
    {
        var results = new List<AwsInternetGatewayInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeInternetGatewaysAsync(
                new DescribeInternetGatewaysRequest
                {
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var gateway in response.InternetGateways ?? [])
            {
                var tags = ToTagDictionary(gateway.Tags);

                results.Add(new AwsInternetGatewayInfo
                {
                    InternetGatewayId =
                        gateway.InternetGatewayId ?? "",
                    Name = GetName(tags),
                    Region = region,
                    Attachments = (gateway.Attachments ?? [])
                        .Select(attachment =>
                            new AwsInternetGatewayAttachmentInfo
                            {
                                VpcId = attachment.VpcId ?? "",
                                State = attachment.State?.Value ?? ""
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

    private static async Task<IReadOnlyList<AwsNatGatewayInfo>>
        GetAllNatGatewaysAsync(
            Amazon.EC2.IAmazonEC2 client,
            string region,
            CancellationToken cancellationToken)
    {
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
                var tags = ToTagDictionary(gateway.Tags);

                results.Add(new AwsNatGatewayInfo
                {
                    NatGatewayId = gateway.NatGatewayId ?? "",
                    Name = GetName(tags),
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

    private static async Task<IReadOnlyList<AwsSecurityGroupInfo>>
        GetAllSecurityGroupsAsync(
            Amazon.EC2.IAmazonEC2 client,
            string region,
            CancellationToken cancellationToken)
    {
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
                var tags = ToTagDictionary(group.Tags);

                results.Add(new AwsSecurityGroupInfo
                {
                    GroupId = group.GroupId ?? "",
                    GroupName = group.GroupName ?? "",
                    Name = GetName(tags),
                    Description = group.Description ?? "",
                    VpcId = group.VpcId ?? "",
                    OwnerId = group.OwnerId ?? "",
                    Region = region,
                    IngressRules = (group.IpPermissions ?? [])
                        .Select(ToSecurityGroupRuleInfo)
                        .ToList(),
                    EgressRules = (group.IpPermissionsEgress ?? [])
                        .Select(ToSecurityGroupRuleInfo)
                        .ToList(),
                    Tags = tags
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }

    private static AwsSecurityGroupRuleInfo ToSecurityGroupRuleInfo(
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

    private static async Task<IReadOnlyList<AwsNetworkAclInfo>>
        GetAllNetworkAclsAsync(
            Amazon.EC2.IAmazonEC2 client,
            string region,
            CancellationToken cancellationToken)
    {
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
                var tags = ToTagDictionary(acl.Tags);

                results.Add(new AwsNetworkAclInfo
                {
                    NetworkAclId = acl.NetworkAclId ?? "",
                    Name = GetName(tags),
                    VpcId = acl.VpcId ?? "",
                    OwnerId = acl.OwnerId ?? "",
                    IsDefault = acl.IsDefault ?? false,
                    Region = region,
                    Associations = (acl.Associations ?? [])
                        .Select(ToNetworkAclAssociationInfo)
                        .OrderBy(association => association.SubnetId)
                        .ToList(),
                    Entries = (acl.Entries ?? [])
                        .Select(ToNetworkAclEntryInfo)
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

    private static AwsNetworkAclAssociationInfo
        ToNetworkAclAssociationInfo(
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

    private static AwsNetworkAclEntryInfo ToNetworkAclEntryInfo(
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

    private static async Task<IReadOnlyList<AwsVpcEndpointInfo>>
        GetAllVpcEndpointsAsync(
            Amazon.EC2.IAmazonEC2 client,
            string region,
            CancellationToken cancellationToken)
    {
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
                var tags = ToTagDictionary(endpoint.Tags);

                results.Add(new AwsVpcEndpointInfo
                {
                    VpcEndpointId = endpoint.VpcEndpointId ?? "",
                    Name = GetName(tags),
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

    private static async Task<IReadOnlyList<AwsElasticIpInfo>>
        GetAllElasticIpsAsync(
            Amazon.EC2.IAmazonEC2 client,
            string region,
            CancellationToken cancellationToken)
    {
        var response = await client.DescribeAddressesAsync(
            new DescribeAddressesRequest(),
            cancellationToken);

        return (response.Addresses ?? [])
            .Select(address =>
            {
                var tags = ToTagDictionary(address.Tags);

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
                    Name = GetName(tags),
                    Region = region,
                    Tags = tags
                };
            })
            .ToList();
    }

    private static async Task<IReadOnlyList<AwsNetworkInterfaceInfo>>
        GetAllNetworkInterfacesAsync(
            Amazon.EC2.IAmazonEC2 client,
            string region,
            CancellationToken cancellationToken)
    {
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
                var tags = ToTagDictionary(networkInterface.TagSet);

                results.Add(new AwsNetworkInterfaceInfo
                {
                    NetworkInterfaceId =
                        networkInterface.NetworkInterfaceId ?? "",
                    Name = GetName(tags),
                    Description = networkInterface.Description ?? "",
                    InterfaceType =
                        networkInterface.InterfaceType?.Value ?? "",
                    Status = networkInterface.Status?.Value ?? "",
                    VpcId = networkInterface.VpcId ?? "",
                    SubnetId = networkInterface.SubnetId ?? "",
                    AvailabilityZone =
                        networkInterface.AvailabilityZone ?? "",
                    OwnerId = networkInterface.OwnerId ?? "",
                    RequesterId = networkInterface.RequesterId ?? "",
                    RequesterManaged =
                        networkInterface.RequesterManaged ?? false,
                    SourceDestinationCheck =
                        networkInterface.SourceDestCheck ?? false,
                    MacAddress = networkInterface.MacAddress ?? "",
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
                            .Select(privateIp =>
                                new AwsNetworkInterfacePrivateIpInfo
                                {
                                    PrivateIpAddress =
                                        privateIp.PrivateIpAddress ?? "",
                                    IsPrimary =
                                        privateIp.Primary ?? false,
                                    PrivateDnsName =
                                        privateIp.PrivateDnsName ?? "",
                                    PublicIp =
                                        privateIp.Association?
                                            .PublicIp ?? "",
                                    PublicDnsName =
                                        privateIp.Association?
                                            .PublicDnsName ?? "",
                                    AllocationId =
                                        privateIp.Association?
                                            .AllocationId ?? "",
                                    AssociationId =
                                        privateIp.Association?
                                            .AssociationId ?? ""
                                })
                            .ToList(),
                    Attachment = networkInterface.Attachment is null
                        ? null
                        : new AwsNetworkInterfaceAttachmentInfo
                        {
                            AttachmentId =
                                networkInterface.Attachment
                                    .AttachmentId ?? "",
                            InstanceId =
                                networkInterface.Attachment
                                    .InstanceId ?? "",
                            InstanceOwnerId =
                                networkInterface.Attachment
                                    .InstanceOwnerId ?? "",
                            DeviceIndex =
                                networkInterface.Attachment
                                    .DeviceIndex ?? 0,
                            NetworkCardIndex =
                                networkInterface.Attachment
                                    .NetworkCardIndex ?? 0,
                            Status =
                                networkInterface.Attachment
                                    .Status?.Value ?? "",
                            DeleteOnTermination =
                                networkInterface.Attachment
                                    .DeleteOnTermination ?? false,
                            AttachTime =
                                networkInterface.Attachment.AttachTime
                        },
                    Tags = tags
                });
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }

    private static AwsRouteTableAssociationInfo ToAssociationInfo(
        RouteTableAssociation association)
    {
        return new AwsRouteTableAssociationInfo
        {
            AssociationId =
                association.RouteTableAssociationId ?? "",
            SubnetId = association.SubnetId ?? "",
            GatewayId = association.GatewayId ?? "",
            IsMain = association.Main == true,
            State = association.AssociationState?
                .State?
                .Value ?? ""
        };
    }

    private static AwsRouteInfo ToRouteInfo(Ec2Route route)
    {
        var destination =
            FirstNonEmpty(
                route.DestinationCidrBlock,
                route.DestinationIpv6CidrBlock,
                route.DestinationPrefixListId);

        var (targetType, targetId) = GetRouteTarget(route);

        return new AwsRouteInfo
        {
            Destination = destination,
            TargetType = targetType,
            TargetId = targetId,
            State = route.State?.Value ?? "",
            Origin = route.Origin?.Value ?? ""
        };
    }

    private static (string TargetType, string TargetId) GetRouteTarget(
        Ec2Route route)
    {
        if (!string.IsNullOrWhiteSpace(route.GatewayId))
            return ("Gateway", route.GatewayId);

        if (!string.IsNullOrWhiteSpace(route.NatGatewayId))
            return ("NatGateway", route.NatGatewayId);

        if (!string.IsNullOrWhiteSpace(route.InstanceId))
            return ("Instance", route.InstanceId);

        if (!string.IsNullOrWhiteSpace(route.NetworkInterfaceId))
            return ("NetworkInterface", route.NetworkInterfaceId);

        if (!string.IsNullOrWhiteSpace(route.TransitGatewayId))
            return ("TransitGateway", route.TransitGatewayId);

        if (!string.IsNullOrWhiteSpace(route.VpcPeeringConnectionId))
            return ("VpcPeeringConnection", route.VpcPeeringConnectionId);

        if (!string.IsNullOrWhiteSpace(
                route.EgressOnlyInternetGatewayId))
        {
            return (
                "EgressOnlyInternetGateway",
                route.EgressOnlyInternetGatewayId);
        }

        return ("Unknown", "");
    }

    private static IReadOnlyDictionary<string, string> ToTagDictionary(
        List<Tag>? tags)
    {
        if (tags is null || tags.Count == 0)
        {
            return new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
        }

        return tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag.Key))
            .GroupBy(
                tag => tag.Key,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Last().Value ?? "",
                StringComparer.OrdinalIgnoreCase);
    }

    private static string GetName(
        IReadOnlyDictionary<string, string> tags)
    {
        return tags.TryGetValue("Name", out var name)
            ? name
            : "";
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(
                   value => !string.IsNullOrWhiteSpace(value))
               ?? "";
    }

    private sealed class RegionalNetworkingResult
    {
        public IReadOnlyList<AwsVpcInfo> Vpcs { get; init; } =
            Array.Empty<AwsVpcInfo>();

        public IReadOnlyList<AwsSubnetInfo> Subnets { get; init; } =
            Array.Empty<AwsSubnetInfo>();

        public IReadOnlyList<AwsRouteTableInfo> RouteTables { get; init; } =
            Array.Empty<AwsRouteTableInfo>();

        public IReadOnlyList<AwsInternetGatewayInfo> InternetGateways
        { get; init; } =
            Array.Empty<AwsInternetGatewayInfo>();

        public IReadOnlyList<AwsNatGatewayInfo> NatGateways { get; init; } =
            Array.Empty<AwsNatGatewayInfo>();

        public IReadOnlyList<AwsSecurityGroupInfo> SecurityGroups
        { get; init; } =
            Array.Empty<AwsSecurityGroupInfo>();

        public IReadOnlyList<AwsNetworkAclInfo> NetworkAcls { get; init; } =
            Array.Empty<AwsNetworkAclInfo>();

        public IReadOnlyList<AwsVpcEndpointInfo> VpcEndpoints
        { get; init; } =
            Array.Empty<AwsVpcEndpointInfo>();

        public IReadOnlyList<AwsElasticIpInfo> ElasticIps { get; init; } =
            Array.Empty<AwsElasticIpInfo>();

        public IReadOnlyList<AwsNetworkInterfaceInfo> NetworkInterfaces
        { get; init; } =
            Array.Empty<AwsNetworkInterfaceInfo>();

        public IReadOnlyList<string> Warnings { get; init; } =
            Array.Empty<string>();
    }
}
