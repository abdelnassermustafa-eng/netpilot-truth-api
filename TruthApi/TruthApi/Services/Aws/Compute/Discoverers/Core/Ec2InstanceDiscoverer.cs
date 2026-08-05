using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Compute;
using TruthApi.Services.Aws.Networking.Infrastructure;

namespace TruthApi.Services.Aws.Compute.Discoverers.Core;

/// <summary>
/// Discovers all EC2 instances in one AWS Region.
/// </summary>
public sealed class Ec2InstanceDiscoverer
{
    public async Task<IReadOnlyList<AwsEc2InstanceInfo>> DiscoverAsync(
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

        var results = new List<AwsEc2InstanceInfo>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeInstancesAsync(
                new DescribeInstancesRequest
                {
                    NextToken = nextToken
                },
                cancellationToken);

            foreach (var reservation in response.Reservations ?? [])
            {
                foreach (var instance in reservation.Instances ?? [])
                {
                    var tags = AwsTagHelper.ToDictionary(instance.Tags);

                    results.Add(new AwsEc2InstanceInfo
                    {
                        InstanceId = instance.InstanceId ?? "",
                        Name = AwsTagHelper.GetName(tags),
                        ImageId = instance.ImageId ?? "",
                        InstanceType =
                            instance.InstanceType?.Value ?? "",
                        Architecture =
                            instance.Architecture?.Value ?? "",
                        Platform =
                            instance.Platform?.Value ?? "",
                        PlatformDetails =
                            instance.PlatformDetails ?? "",
                        State =
                            instance.State?.Name?.Value ?? "",
                        StateCode =
                            instance.State?.Code ?? 0,
                        StateTransitionReason =
                            instance.StateTransitionReason ?? "",
                        LaunchTime = instance.LaunchTime,
                        VpcId = instance.VpcId ?? "",
                        SubnetId = instance.SubnetId ?? "",
                        PrivateIpAddress =
                            instance.PrivateIpAddress ?? "",
                        PrivateDnsName =
                            instance.PrivateDnsName ?? "",
                        PublicIpAddress =
                            instance.PublicIpAddress ?? "",
                        PublicDnsName =
                            instance.PublicDnsName ?? "",
                        KeyName = instance.KeyName ?? "",
                        VirtualizationType =
                            instance.VirtualizationType?.Value ?? "",
                        Hypervisor =
                            instance.Hypervisor?.Value ?? "",
                        RootDeviceType =
                            instance.RootDeviceType?.Value ?? "",
                        RootDeviceName =
                            instance.RootDeviceName ?? "",
                        SourceDestinationCheck =
                            instance.SourceDestCheck ?? false,
                        EbsOptimized =
                            instance.EbsOptimized ?? false,
                        EnaSupport =
                            instance.EnaSupport ?? false,
                        ClientToken =
                            instance.ClientToken ?? "",
                        Region = region,

                        Placement = ToPlacementInfo(
                            instance.Placement),

                        IamInstanceProfile =
                            ToInstanceProfileInfo(
                                instance.IamInstanceProfile),

                        NetworkInterfaceIds =
                            (instance.NetworkInterfaces ?? [])
                                .Select(networkInterface =>
                                    networkInterface
                                        .NetworkInterfaceId ?? "")
                                .Where(id =>
                                    !string.IsNullOrWhiteSpace(id))
                                .ToList(),

                        SecurityGroups =
                            (instance.SecurityGroups ?? [])
                                .Select(group =>
                                    new AwsInstanceSecurityGroupInfo
                                    {
                                        GroupId =
                                            group.GroupId ?? "",
                                        GroupName =
                                            group.GroupName ?? ""
                                    })
                                .ToList(),

                        BlockDevices =
                            (instance.BlockDeviceMappings ?? [])
                                .Select(ToBlockDeviceInfo)
                                .ToList(),

                        Tags = tags
                    });
                }
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }

    private static AwsInstancePlacementInfo ToPlacementInfo(
        Placement? placement)
    {
        if (placement is null)
        {
            return new AwsInstancePlacementInfo();
        }

        return new AwsInstancePlacementInfo
        {
            AvailabilityZone =
                placement.AvailabilityZone ?? "",
            Tenancy =
                placement.Tenancy?.Value ?? "",
            HostId =
                placement.HostId ?? "",
            Affinity =
                placement.Affinity ?? "",
            GroupName =
                placement.GroupName ?? "",
            PartitionNumber =
                placement.PartitionNumber
        };
    }

    private static AwsInstanceProfileInfo? ToInstanceProfileInfo(
        IamInstanceProfile? profile)
    {
        if (profile is null)
        {
            return null;
        }

        var arn = profile.Arn ?? "";

        return new AwsInstanceProfileInfo
        {
            Arn = arn,
            Id = profile.Id ?? "",
            Name = ExtractResourceName(arn)
        };
    }

    private static AwsInstanceBlockDeviceInfo ToBlockDeviceInfo(
        InstanceBlockDeviceMapping mapping)
    {
        return new AwsInstanceBlockDeviceInfo
        {
            DeviceName = mapping.DeviceName ?? "",
            VolumeId = mapping.Ebs?.VolumeId ?? "",
            Status = mapping.Ebs?.Status?.Value ?? "",
            DeleteOnTermination =
                mapping.Ebs?.DeleteOnTermination ?? false,
            AttachTime = mapping.Ebs?.AttachTime
        };
    }

    private static string ExtractResourceName(string arn)
    {
        if (string.IsNullOrWhiteSpace(arn))
        {
            return "";
        }

        var slashIndex = arn.LastIndexOf('/');

        return slashIndex >= 0 && slashIndex < arn.Length - 1
            ? arn[(slashIndex + 1)..]
            : arn;
    }
}
