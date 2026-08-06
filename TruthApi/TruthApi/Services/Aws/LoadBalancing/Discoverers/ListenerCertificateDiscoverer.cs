using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using TruthApi.Models.Aws.LoadBalancing;

namespace TruthApi.Services.Aws.LoadBalancing.Discoverers;

/// <summary>
/// Discovers all certificates attached to known HTTPS and TLS listeners
/// in one AWS Region.
/// </summary>
public sealed class ListenerCertificateDiscoverer
{
    public async Task<IReadOnlyList<AwsListenerCertificateInfo>>
        DiscoverAsync(
            IAmazonElasticLoadBalancingV2 client,
            string region,
            IReadOnlyCollection<AwsListenerInfo> listeners,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(listeners);

        if (string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException(
                "AWS Region is required.",
                nameof(region));
        }

        var eligibleListeners = listeners
            .Where(listener =>
                string.Equals(
                    listener.Protocol,
                    "HTTPS",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    listener.Protocol,
                    "TLS",
                    StringComparison.OrdinalIgnoreCase))
            .Where(listener =>
                !string.IsNullOrWhiteSpace(listener.ListenerArn))
            .GroupBy(
                listener => listener.ListenerArn,
                StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(listener => listener.ListenerArn)
            .ToList();

        if (eligibleListeners.Count == 0)
        {
            return Array.Empty<AwsListenerCertificateInfo>();
        }

        var discoveryTasks = eligibleListeners
            .Select(listener =>
                DiscoverForListenerAsync(
                    client,
                    region,
                    listener.ListenerArn,
                    cancellationToken))
            .ToArray();

        var results = await Task.WhenAll(discoveryTasks);

        // AWS may return the default certificate twice: once as the
        // default and once as an entry in the certificate list.
        return results
            .SelectMany(result => result)
            .GroupBy(
                certificate => new
                {
                    certificate.ListenerArn,
                    certificate.CertificateArn
                })
            .Select(group =>
                new AwsListenerCertificateInfo
                {
                    ListenerArn = group.Key.ListenerArn,
                    CertificateArn = group.Key.CertificateArn,
                    IsDefault = group.Any(
                        certificate => certificate.IsDefault),
                    Region = region
                })
            .OrderBy(certificate => certificate.ListenerArn)
            .ThenByDescending(certificate => certificate.IsDefault)
            .ThenBy(certificate => certificate.CertificateArn)
            .ToList();
    }

    private static async Task<
        IReadOnlyList<AwsListenerCertificateInfo>>
        DiscoverForListenerAsync(
            IAmazonElasticLoadBalancingV2 client,
            string region,
            string listenerArn,
            CancellationToken cancellationToken)
    {
        var results = new List<AwsListenerCertificateInfo>();
        string? marker = null;

        do
        {
            var response =
                await client.DescribeListenerCertificatesAsync(
                    new DescribeListenerCertificatesRequest
                    {
                        ListenerArn = listenerArn,
                        Marker = marker,
                        PageSize = 100
                    },
                    cancellationToken);

            results.AddRange(
                (response.Certificates ?? [])
                    .Where(certificate =>
                        !string.IsNullOrWhiteSpace(
                            certificate.CertificateArn))
                    .Select(certificate =>
                        new AwsListenerCertificateInfo
                        {
                            ListenerArn = listenerArn,
                            CertificateArn =
                                certificate.CertificateArn ?? "",
                            IsDefault =
                                certificate.IsDefault ?? false,
                            Region = region
                        }));

            marker = response.NextMarker;
        }
        while (!string.IsNullOrWhiteSpace(marker));

        return results;
    }
}
