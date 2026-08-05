using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Compute;
using TruthApi.Services.Aws.Networking.Infrastructure;

namespace TruthApi.Services.Aws.Compute.Discoverers.Core;

/// <summary>
/// Discovers EC2 Launch Templates and their default and latest versions
/// in one AWS Region.
/// </summary>
public sealed class LaunchTemplateDiscoverer
{
    public async Task<IReadOnlyList<AwsLaunchTemplateInfo>> DiscoverAsync(
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

        var templates = await GetAllTemplatesAsync(
            client,
            cancellationToken);

        var results = new List<AwsLaunchTemplateInfo>();

        foreach (var template in templates)
        {
            var versions = await GetSelectedVersionsAsync(
                client,
                template.LaunchTemplateId ?? "",
                cancellationToken);

            var defaultVersion = versions.FirstOrDefault(
                version =>
                    version.VersionNumber ==
                    template.DefaultVersionNumber);

            var latestVersion = versions.FirstOrDefault(
                version =>
                    version.VersionNumber ==
                    template.LatestVersionNumber);

            var tags = AwsTagHelper.ToDictionary(template.Tags);

            results.Add(new AwsLaunchTemplateInfo
            {
                LaunchTemplateId =
                    template.LaunchTemplateId ?? "",
                LaunchTemplateName =
                    template.LaunchTemplateName ?? "",
                CreatedBy =
                    template.CreatedBy ?? "",
                CreatedAt =
                    template.CreateTime,
                DefaultVersionNumber =
                    template.DefaultVersionNumber ?? 0,
                LatestVersionNumber =
                    template.LatestVersionNumber ?? 0,
                Region = region,
                DefaultVersion =
                    defaultVersion is null
                        ? null
                        : ToVersionInfo(defaultVersion),
                LatestVersion =
                    latestVersion is null
                        ? null
                        : ToVersionInfo(latestVersion),
                Tags = tags
            });
        }

        return results
            .OrderBy(template => template.LaunchTemplateName)
            .ThenBy(template => template.LaunchTemplateId)
            .ToList();
    }

    private static async Task<IReadOnlyList<LaunchTemplate>>
        GetAllTemplatesAsync(
            IAmazonEC2 client,
            CancellationToken cancellationToken)
    {
        var results = new List<LaunchTemplate>();
        string? nextToken = null;

        do
        {
            var response = await client.DescribeLaunchTemplatesAsync(
                new DescribeLaunchTemplatesRequest
                {
                    MaxResults = 200,
                    NextToken = nextToken
                },
                cancellationToken);

            results.AddRange(response.LaunchTemplates ?? []);

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results;
    }

    private static async Task<IReadOnlyList<LaunchTemplateVersion>>
        GetSelectedVersionsAsync(
            IAmazonEC2 client,
            string launchTemplateId,
            CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(launchTemplateId))
        {
            return Array.Empty<LaunchTemplateVersion>();
        }

        var results = new List<LaunchTemplateVersion>();
        string? nextToken = null;

        do
        {
            var response =
                await client.DescribeLaunchTemplateVersionsAsync(
                    new DescribeLaunchTemplateVersionsRequest
                    {
                        LaunchTemplateId = launchTemplateId,
                        Versions = ["$Default", "$Latest"],
                        MaxResults = 200,
                        NextToken = nextToken
                    },
                    cancellationToken);

            results.AddRange(
                response.LaunchTemplateVersions ?? []);

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrWhiteSpace(nextToken));

        return results
            .GroupBy(version => version.VersionNumber)
            .Select(group => group.First())
            .ToList();
    }

    private static AwsLaunchTemplateVersionInfo ToVersionInfo(
        LaunchTemplateVersion version)
    {
        var data = version.LaunchTemplateData;

        return new AwsLaunchTemplateVersionInfo
        {
            VersionNumber =
                version.VersionNumber ?? 0,
            VersionDescription =
                version.VersionDescription ?? "",
            IsDefaultVersion =
                version.DefaultVersion ?? false,
            CreatedAt =
                version.CreateTime,
            CreatedBy =
                version.CreatedBy ?? "",
            ImageId =
                data?.ImageId ?? "",
            InstanceType =
                data?.InstanceType?.Value ?? "",
            KeyName =
                data?.KeyName ?? "",
            IamInstanceProfileArn =
                data?.IamInstanceProfile?.Arn ?? "",
            IamInstanceProfileName =
                data?.IamInstanceProfile?.Name ?? "",
            HasUserData =
                !string.IsNullOrWhiteSpace(data?.UserData),
            EbsOptimized =
                data?.EbsOptimized ?? false,
            DisableApiTermination =
                data?.DisableApiTermination ?? false,
            InstanceInitiatedShutdownBehavior =
                data?.InstanceInitiatedShutdownBehavior?.Value ?? "",
            MetadataHttpTokens =
                data?.MetadataOptions?.HttpTokens?.Value ?? "",
            MetadataHttpEndpoint =
                data?.MetadataOptions?.HttpEndpoint?.Value ?? "",
            MetadataHttpPutResponseHopLimit =
                data?.MetadataOptions?.HttpPutResponseHopLimit,
            SecurityGroupIds =
                (data?.SecurityGroupIds ?? [])
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .ToList(),
            SecurityGroupNames =
                (data?.SecurityGroups ?? [])
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .ToList()
        };
    }
}
