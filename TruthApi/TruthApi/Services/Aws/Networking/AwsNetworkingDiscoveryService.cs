using Amazon.EC2.Model;
using TruthApi.Models.Aws.Networking;
using TruthApi.Services.Aws.Networking.Discoverers.Core;
using TruthApi.Services.Aws.Networking.Discoverers.Security;
using TruthApi.Services.Aws.Networking.Discoverers.Connectivity;

namespace TruthApi.Services.Aws.Networking;

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
    private readonly VpcDiscoverer _vpcDiscoverer;
    private readonly SubnetDiscoverer _subnetDiscoverer;
    private readonly RouteTableDiscoverer _routeTableDiscoverer;
    private readonly InternetGatewayDiscoverer _internetGatewayDiscoverer;
    private readonly NatGatewayDiscoverer _natGatewayDiscoverer;
    private readonly VpcEndpointDiscoverer _vpcEndpointDiscoverer;
    private readonly ElasticIpDiscoverer _elasticIpDiscoverer;
    private readonly NetworkInterfaceDiscoverer _networkInterfaceDiscoverer;
    private readonly SecurityGroupDiscoverer _securityGroupDiscoverer;
    private readonly NetworkAclDiscoverer _networkAclDiscoverer;

    public AwsNetworkingDiscoveryService(
        AwsClientFactory clientFactory,
        AwsIdentityService identityService,
        VpcDiscoverer vpcDiscoverer,
        SubnetDiscoverer subnetDiscoverer,
        RouteTableDiscoverer routeTableDiscoverer,
        InternetGatewayDiscoverer internetGatewayDiscoverer,
        NatGatewayDiscoverer natGatewayDiscoverer,
        VpcEndpointDiscoverer vpcEndpointDiscoverer,
        ElasticIpDiscoverer elasticIpDiscoverer,
        NetworkInterfaceDiscoverer networkInterfaceDiscoverer,
        SecurityGroupDiscoverer securityGroupDiscoverer,
        NetworkAclDiscoverer networkAclDiscoverer)
    {
        _clientFactory = clientFactory;
        _identityService = identityService;
        _vpcDiscoverer = vpcDiscoverer;
        _subnetDiscoverer = subnetDiscoverer;
        _routeTableDiscoverer = routeTableDiscoverer;
        _internetGatewayDiscoverer = internetGatewayDiscoverer;
        _natGatewayDiscoverer = natGatewayDiscoverer;
        _vpcEndpointDiscoverer = vpcEndpointDiscoverer;
        _elasticIpDiscoverer = elasticIpDiscoverer;
        _networkInterfaceDiscoverer = networkInterfaceDiscoverer;
        _securityGroupDiscoverer = securityGroupDiscoverer;
        _networkAclDiscoverer = networkAclDiscoverer;
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

            var vpcsTask =
                _vpcDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);
            var subnetsTask =
                _subnetDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);
            var routeTablesTask =
                _routeTableDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var internetGatewaysTask =
                _internetGatewayDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var natGatewaysTask =
                _natGatewayDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var securityGroupsTask =
                _securityGroupDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var networkAclsTask =
                _networkAclDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var vpcEndpointsTask =
                _vpcEndpointDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var elasticIpsTask =
                _elasticIpDiscoverer.DiscoverAsync(
                    client,
                    region,
                    cancellationToken);

            var networkInterfacesTask =
                _networkInterfaceDiscoverer.DiscoverAsync(
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
