using Amazon.ElasticLoadBalancingV2;
using Amazon.ElasticLoadBalancingV2.Model;
using TruthApi.Models.Aws.LoadBalancing;
using Elbv2Action = Amazon.ElasticLoadBalancingV2.Model.Action;

namespace TruthApi.Services.Aws.LoadBalancing.Discoverers;

/// <summary>
/// Discovers listeners for known ELBv2 load balancers in one Region.
/// </summary>
public sealed class ListenerDiscoverer
{
    public async Task<IReadOnlyList<AwsListenerInfo>> DiscoverAsync(
        IAmazonElasticLoadBalancingV2 client,
        string region,
        IReadOnlyCollection<string> loadBalancerArns,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(loadBalancerArns);

        if (string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException(
                "AWS Region is required.",
                nameof(region));
        }

        var arns = loadBalancerArns
            .Where(arn => !string.IsNullOrWhiteSpace(arn))
            .Select(arn => arn.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(arn => arn)
            .ToList();

        if (arns.Count == 0)
        {
            return Array.Empty<AwsListenerInfo>();
        }

        var tasks = arns
            .Select(arn =>
                DiscoverForLoadBalancerAsync(
                    client,
                    region,
                    arn,
                    cancellationToken))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        return results
            .SelectMany(result => result)
            .OrderBy(listener => listener.LoadBalancerArn)
            .ThenBy(listener => listener.Port)
            .ThenBy(listener => listener.Protocol)
            .ThenBy(listener => listener.ListenerArn)
            .ToList();
    }

    private static async Task<IReadOnlyList<AwsListenerInfo>>
        DiscoverForLoadBalancerAsync(
            IAmazonElasticLoadBalancingV2 client,
            string region,
            string loadBalancerArn,
            CancellationToken cancellationToken)
    {
        var results = new List<AwsListenerInfo>();
        string? marker = null;

        do
        {
            var response = await client.DescribeListenersAsync(
                new DescribeListenersRequest
                {
                    LoadBalancerArn = loadBalancerArn,
                    Marker = marker,
                    PageSize = 100
                },
                cancellationToken);

            foreach (var listener in response.Listeners ?? [])
            {
                results.Add(
                    new AwsListenerInfo
                    {
                        ListenerArn = listener.ListenerArn ?? "",
                        LoadBalancerArn =
                            listener.LoadBalancerArn ??
                            loadBalancerArn,
                        Protocol =
                            listener.Protocol?.Value ?? "",
                        Port = listener.Port,
                        SslPolicy = listener.SslPolicy ?? "",
                        Region = region,
                        AlpnPolicies =
                            (listener.AlpnPolicy ?? [])
                                .Where(value =>
                                    !string.IsNullOrWhiteSpace(value))
                                .ToList(),
                        Certificates =
                            (listener.Certificates ?? [])
                                .Select(ToCertificateInfo)
                                .ToList(),
                        DefaultActions =
                            (listener.DefaultActions ?? [])
                                .Select(ToActionInfo)
                                .OrderBy(action => action.Order)
                                .ToList(),
                        MutualAuthentication =
                            ToMutualAuthenticationInfo(
                                listener.MutualAuthentication)
                    });
            }

            marker = response.NextMarker;
        }
        while (!string.IsNullOrWhiteSpace(marker));

        return results;
    }

    private static AwsListenerCertificateInfo ToCertificateInfo(
        Certificate certificate)
    {
        return new AwsListenerCertificateInfo
        {
            CertificateArn = certificate.CertificateArn ?? "",
            IsDefault = certificate.IsDefault ?? false
        };
    }

    private static AwsListenerActionInfo ToActionInfo(
        Elbv2Action action)
    {
        return new AwsListenerActionInfo
        {
            Order = action.Order,
            Type = action.Type?.Value ?? "",
            TargetGroupArn = action.TargetGroupArn ?? "",

            RedirectProtocol =
                action.RedirectConfig?.Protocol ?? "",
            RedirectHost =
                action.RedirectConfig?.Host ?? "",
            RedirectPort =
                action.RedirectConfig?.Port ?? "",
            RedirectPath =
                action.RedirectConfig?.Path ?? "",
            RedirectQuery =
                action.RedirectConfig?.Query ?? "",
            RedirectStatusCode =
                action.RedirectConfig?.StatusCode?.Value ?? "",

            FixedResponseContentType =
                action.FixedResponseConfig?.ContentType ?? "",
            FixedResponseMessageBody =
                action.FixedResponseConfig?.MessageBody ?? "",
            FixedResponseStatusCode =
                action.FixedResponseConfig?.StatusCode ?? "",

            AuthenticateOidcIssuer =
                action.AuthenticateOidcConfig?.Issuer ?? "",
            AuthenticateOidcClientId =
                action.AuthenticateOidcConfig?.ClientId ?? "",

            AuthenticateCognitoUserPoolArn =
                action.AuthenticateCognitoConfig?
                    .UserPoolArn ?? "",
            AuthenticateCognitoUserPoolClientId =
                action.AuthenticateCognitoConfig?
                    .UserPoolClientId ?? "",
            AuthenticateCognitoUserPoolDomain =
                action.AuthenticateCognitoConfig?
                    .UserPoolDomain ?? ""
        };
    }

    private static AwsListenerMutualAuthenticationInfo?
        ToMutualAuthenticationInfo(
            MutualAuthenticationAttributes? mutualAuthentication)
    {
        if (mutualAuthentication is null)
        {
            return null;
        }

        return new AwsListenerMutualAuthenticationInfo
        {
            Mode = mutualAuthentication.Mode ?? "",
            TrustStoreArn =
                mutualAuthentication.TrustStoreArn ?? "",
            IgnoreClientCertificateExpiry =
                mutualAuthentication
                    .IgnoreClientCertificateExpiry ?? false,
            TrustStoreAssociationStatus =
                mutualAuthentication
                    .TrustStoreAssociationStatus ?? "",
            AdvertiseTrustStoreCaNames =
                mutualAuthentication
                    .AdvertiseTrustStoreCaNames ?? ""
        };
    }
}
