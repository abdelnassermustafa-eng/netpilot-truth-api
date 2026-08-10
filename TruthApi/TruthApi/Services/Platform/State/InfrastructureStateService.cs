using System.Reflection;
using TruthApi.Models.Platform.State;
using TruthApi.Services.Aws;
using TruthApi.Services.Aws.Compute;
using TruthApi.Services.Aws.LoadBalancing;
using TruthApi.Services.Aws.Networking;

namespace TruthApi.Services.Platform.State;

public sealed class InfrastructureStateService
{
    private readonly AwsResourceDiscoveryService
        _environmentDiscovery;

    private readonly AwsNetworkingDiscoveryService
        _networkingDiscovery;

    private readonly AwsComputeDiscoveryService
        _computeDiscovery;

    private readonly AwsLoadBalancingDiscoveryService
        _loadBalancingDiscovery;

    private readonly AwsInventoryNormalizer _normalizer;

    private readonly AwsRelationshipBuilder
        _relationshipBuilder;

    public InfrastructureStateService(
        AwsResourceDiscoveryService environmentDiscovery,
        AwsNetworkingDiscoveryService networkingDiscovery,
        AwsComputeDiscoveryService computeDiscovery,
        AwsLoadBalancingDiscoveryService loadBalancingDiscovery,
        AwsInventoryNormalizer normalizer,
        AwsRelationshipBuilder relationshipBuilder)
    {
        _environmentDiscovery = environmentDiscovery;
        _networkingDiscovery = networkingDiscovery;
        _computeDiscovery = computeDiscovery;
        _loadBalancingDiscovery = loadBalancingDiscovery;
        _normalizer = normalizer;
        _relationshipBuilder = relationshipBuilder;
    }

    public async Task<InfrastructureState> DiscoverAsync(
        IReadOnlyCollection<string>? regions,
        CancellationToken cancellationToken = default)
    {
        var environmentTask =
            _environmentDiscovery.DiscoverEnvironmentAsync(
                includeDisabledRegions: true,
                cancellationToken);

        var networkingTask =
            _networkingDiscovery.DiscoverAsync(
                regions?.ToList(),
                cancellationToken);

        var computeTask =
            _computeDiscovery.DiscoverAsync(
                regions?.ToList(),
                cancellationToken);

        var loadBalancingTask =
            _loadBalancingDiscovery.DiscoverAsync(
                regions?.ToList(),
                cancellationToken);

        await Task.WhenAll(
            environmentTask,
            networkingTask,
            computeTask,
            loadBalancingTask);

        var environment = await environmentTask;
        var networking = await networkingTask;
        var compute = await computeTask;
        var loadBalancing = await loadBalancingTask;

        var discoveredAt = DateTimeOffset.UtcNow;

        var accountId = FirstNonEmpty(
            networking.AccountId,
            compute.AccountId,
            ReadString(environment.Identity, "AccountId"),
            ReadString(environment.Identity, "Account"));

        var resources = new List<InfrastructureResource>();

        resources.AddRange(
            _normalizer.Normalize(
                networking,
                accountId,
                "networking",
                discoveredAt));

        resources.AddRange(
            _normalizer.Normalize(
                compute,
                accountId,
                "compute",
                discoveredAt));

        resources.AddRange(
            _normalizer.Normalize(
                loadBalancing,
                accountId,
                "load-balancing",
                discoveredAt));

        var relationships =
            _relationshipBuilder.Build(
                resources);

        var warnings = networking.Warnings
            .Concat(compute.Warnings)
            .Concat(loadBalancing.Warnings)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var domains = resources
            .GroupBy(
                resource => resource.DomainId,
                StringComparer.OrdinalIgnoreCase)
            .Select(group => new InfrastructureDomain
            {
                Id = group.Key,
                DisplayName = DisplayName(group.Key),
                IconKey = IconFor(group.Key),
                AccentKey = group
                    .Select(resource => resource.AccentKey)
                    .FirstOrDefault() ?? "default",
                ResourceCount = group.Count(),
                ResourceTypes = group
                    .Select(resource => resource.ResourceType)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value)
                    .ToList()
            })
            .OrderBy(domain => domain.DisplayName)
            .ToList();

        return new InfrastructureState
        {
            Context = new PlatformContext
            {
                ProviderId = "aws",
                ProviderName = "Amazon Web Services",
                AccountId = accountId,
                AccountName = FirstNonEmpty(
                    ReadString(
                        environment.Identity,
                        "AccountAlias"),
                    accountId),
                IdentityId = FirstNonEmpty(
                    ReadString(
                        environment.Identity,
                        "UserId"),
                    ReadString(
                        environment.Identity,
                        "PrincipalId")),
                IdentityArn = ReadString(
                    environment.Identity,
                    "Arn"),
                DefaultLocation =
                    environment.DefaultRegion,
                Locations = environment.Regions
                    .Select(region =>
                        FirstNonEmpty(
                            ReadString(region, "RegionName"),
                            ReadString(region, "Name")))
                    .Where(value =>
                        !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(value => value)
                    .ToList()
            },
            Domains = domains,
            Resources = resources,
            Relationships = relationships,
            Warnings = warnings,
            DiscoveredAt = discoveredAt
        };
    }

    private static string ReadString(
        object? source,
        string propertyName)
    {
        if (source is null)
        {
            return "";
        }

        var property = source
            .GetType()
            .GetProperty(
                propertyName,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.IgnoreCase);

        return Convert.ToString(
                   property?.GetValue(source))
               ?? "";
    }

    private static string DisplayName(string domainId)
    {
        return domainId switch
        {
            "auto-scaling" => "Auto Scaling",
            "load-balancing" => "Load Balancing",
            _ => string.Join(
                " ",
                domainId
                    .Split(
                        '-',
                        StringSplitOptions.RemoveEmptyEntries)
                    .Select(word =>
                        char.ToUpperInvariant(word[0]) +
                        word[1..]))
        };
    }

    private static string IconFor(string domainId)
    {
        return domainId switch
        {
            "networking" => "network",
            "compute" => "compute",
            "auto-scaling" => "scale",
            "load-balancing" => "load-balancer",
            "storage" => "storage",
            "database" => "database",
            "containers" => "containers",
            "security" => "security",
            "observability" => "observability",
            "ai" => "ai",
            _ => "resource"
        };
    }

    private static string FirstNonEmpty(
        params string?[] values)
    {
        return values.FirstOrDefault(
                   value => !string.IsNullOrWhiteSpace(value))
               ?? "";
    }
}
