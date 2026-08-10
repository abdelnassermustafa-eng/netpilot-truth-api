using TruthApi.Models.Platform.Catalog;

namespace TruthApi.Services.Platform.Catalog;

public sealed class AwsPlatformCatalogContributor
    : IPlatformCatalogContributor
{
    public PlatformProviderDefinition GetProviderDefinition()
    {
        return new PlatformProviderDefinition
        {
            Id = "aws",
            DisplayName = "Amazon Web Services",
            IconKey = "aws",
            AccentKey = "orange",
            Domains =
            [
                new PlatformDomainDefinition
                {
                    Id = "identity",
                    DisplayName = "Identity",
                    IconKey = "identity",
                    AccentKey = "amber",
                    ResourceTypes =
                    [
                        Resource("account", "AWS Account"),
                        Resource("principal", "AWS Principal"),
                        Resource("region", "AWS Region")
                    ]
                },
                new PlatformDomainDefinition
                {
                    Id = "networking",
                    DisplayName = "Networking",
                    IconKey = "network",
                    AccentKey = "purple",
                    ResourceTypes =
                    [
                        Resource("vpc", "VPC"),
                        Resource("subnet", "Subnet"),
                        Resource("route-table", "Route Table"),
                        Resource(
                            "internet-gateway",
                            "Internet Gateway"),
                        Resource("nat-gateway", "NAT Gateway"),
                        Resource("vpc-endpoint", "VPC Endpoint"),
                        Resource("elastic-ip", "Elastic IP"),
                        Resource(
                            "network-interface",
                            "Network Interface"),
                        Resource(
                            "security-group",
                            "Security Group"),
                        Resource("network-acl", "Network ACL")
                    ]
                },
                new PlatformDomainDefinition
                {
                    Id = "compute",
                    DisplayName = "Compute",
                    IconKey = "compute",
                    AccentKey = "blue",
                    ResourceTypes =
                    [
                        Resource("instance", "EC2 Instance"),
                        Resource("volume", "EBS Volume"),
                        Resource("snapshot", "EBS Snapshot"),
                        Resource("ami", "AMI"),
                        Resource("key-pair", "Key Pair"),
                        Resource(
                            "launch-template",
                            "Launch Template")
                    ]
                },
                new PlatformDomainDefinition
                {
                    Id = "auto-scaling",
                    DisplayName = "Auto Scaling",
                    IconKey = "scale",
                    AccentKey = "green",
                    ResourceTypes =
                    [
                        Resource(
                            "auto-scaling-group",
                            "Auto Scaling Group"),
                        Resource(
                            "launch-configuration",
                            "Launch Configuration"),
                        Resource(
                            "scaling-policy",
                            "Scaling Policy"),
                        Resource(
                            "scheduled-action",
                            "Scheduled Action"),
                        Resource(
                            "scaling-activity",
                            "Scaling Activity"),
                        Resource(
                            "lifecycle-hook",
                            "Lifecycle Hook"),
                        Resource("warm-pool", "Warm Pool"),
                        Resource(
                            "instance-refresh",
                            "Instance Refresh")
                    ]
                },
                new PlatformDomainDefinition
                {
                    Id = "load-balancing",
                    DisplayName = "Load Balancing",
                    IconKey = "load-balancer",
                    AccentKey = "cyan",
                    ResourceTypes =
                    [
                        Resource(
                            "load-balancer",
                            "Load Balancer"),
                        Resource(
                            "load-balancer-attribute",
                            "Load Balancer Attribute"),
                        Resource("listener", "Listener"),
                        Resource(
                            "listener-rule",
                            "Listener Rule"),
                        Resource(
                            "listener-certificate",
                            "Listener Certificate"),
                        Resource(
                            "target-group",
                            "Target Group"),
                        Resource(
                            "target-health",
                            "Target Health")
                    ]
                }
            ]
        };
    }

    private static PlatformResourceTypeDefinition Resource(
        string id,
        string displayName,
        string iconKey = "resource")
    {
        return new PlatformResourceTypeDefinition
        {
            Id = id,
            DisplayName = displayName,
            IconKey = iconKey
        };
    }
}
