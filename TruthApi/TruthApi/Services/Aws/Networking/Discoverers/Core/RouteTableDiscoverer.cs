using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Networking;
using TruthApi.Services.Aws.Networking.Infrastructure;
using Ec2Route = Amazon.EC2.Model.Route;

namespace TruthApi.Services.Aws.Networking.Discoverers.Core;

/// <summary>
/// Discovers all route tables, routes, and associations in one AWS Region.
/// </summary>
public sealed class RouteTableDiscoverer
{
    public async Task<IReadOnlyList<AwsRouteTableInfo>> DiscoverAsync(
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
                var tags = AwsTagHelper.ToDictionary(routeTable.Tags);
                var associations = routeTable.Associations ?? [];
                var routes = routeTable.Routes ?? [];

                results.Add(new AwsRouteTableInfo
                {
                    RouteTableId = routeTable.RouteTableId ?? "",
                    Name = AwsTagHelper.GetName(tags),
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
        var destination = FirstNonEmpty(
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
        {
            return ("Gateway", route.GatewayId);
        }

        if (!string.IsNullOrWhiteSpace(route.NatGatewayId))
        {
            return ("NatGateway", route.NatGatewayId);
        }

        if (!string.IsNullOrWhiteSpace(route.InstanceId))
        {
            return ("Instance", route.InstanceId);
        }

        if (!string.IsNullOrWhiteSpace(route.NetworkInterfaceId))
        {
            return ("NetworkInterface", route.NetworkInterfaceId);
        }

        if (!string.IsNullOrWhiteSpace(route.TransitGatewayId))
        {
            return ("TransitGateway", route.TransitGatewayId);
        }

        if (!string.IsNullOrWhiteSpace(route.VpcPeeringConnectionId))
        {
            return (
                "VpcPeeringConnection",
                route.VpcPeeringConnectionId);
        }

        if (!string.IsNullOrWhiteSpace(
                route.EgressOnlyInternetGatewayId))
        {
            return (
                "EgressOnlyInternetGateway",
                route.EgressOnlyInternetGatewayId);
        }

        return ("Unknown", "");
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(
                   value => !string.IsNullOrWhiteSpace(value))
               ?? "";
    }
}
