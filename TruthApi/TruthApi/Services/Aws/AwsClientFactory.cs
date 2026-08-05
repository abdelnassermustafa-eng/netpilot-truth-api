using System.Collections.Concurrent;
using Amazon;
using Amazon.AutoScaling;
using Amazon.EC2;
using Amazon.ElasticLoadBalancingV2;
using Amazon.SecurityToken;
using Microsoft.Extensions.Options;
using TruthApi.Models;

namespace TruthApi.Services.Aws;

/// <summary>
/// Creates and reuses AWS SDK clients.
///
/// Credentials are resolved through the standard AWS SDK credential chain.
/// No access keys are stored by this service.
/// </summary>
public sealed class AwsClientFactory : IDisposable
{
    private readonly ConcurrentDictionary<string, IAmazonEC2> _ec2Clients =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, IAmazonAutoScaling>
        _autoScalingClients =
            new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<
        string,
        IAmazonElasticLoadBalancingV2> _elbv2Clients =
            new(StringComparer.OrdinalIgnoreCase);

    private readonly Lazy<IAmazonSecurityTokenService> _stsClient;

    public AwsClientFactory(IOptions<AwsConfig> awsConfig)
    {
        var configuredRegion = awsConfig.Value.Region?.Trim();

        DefaultRegion = string.IsNullOrWhiteSpace(configuredRegion)
            ? "us-east-1"
            : configuredRegion;

        _stsClient = new Lazy<IAmazonSecurityTokenService>(
            () =>
            {
                var endpoint =
                    RegionEndpoint.GetBySystemName(DefaultRegion);

                return new AmazonSecurityTokenServiceClient(endpoint);
            },
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public string DefaultRegion { get; }

    public IAmazonSecurityTokenService GetStsClient()
    {
        return _stsClient.Value;
    }

    public IAmazonEC2 GetEc2Client(string? regionName = null)
    {
        var effectiveRegion = string.IsNullOrWhiteSpace(regionName)
            ? DefaultRegion
            : regionName.Trim();

        return _ec2Clients.GetOrAdd(
            effectiveRegion,
            static name =>
            {
                var endpoint = RegionEndpoint.GetBySystemName(name);
                return new AmazonEC2Client(endpoint);
            });
    }

    public IAmazonAutoScaling GetAutoScalingClient(
        string? regionName = null)
    {
        var effectiveRegion = string.IsNullOrWhiteSpace(regionName)
            ? DefaultRegion
            : regionName.Trim();

        return _autoScalingClients.GetOrAdd(
            effectiveRegion,
            static name =>
            {
                var endpoint = RegionEndpoint.GetBySystemName(name);
                return new AmazonAutoScalingClient(endpoint);
            });
    }

    public IAmazonElasticLoadBalancingV2 GetElbv2Client(
        string? regionName = null)
    {
        var effectiveRegion = string.IsNullOrWhiteSpace(regionName)
            ? DefaultRegion
            : regionName.Trim();

        return _elbv2Clients.GetOrAdd(
            effectiveRegion,
            static name =>
            {
                var endpoint = RegionEndpoint.GetBySystemName(name);

                return new AmazonElasticLoadBalancingV2Client(
                    endpoint);
            });
    }

    public void Dispose()
    {
        foreach (var client in _ec2Clients.Values)
        {
            client.Dispose();
        }

        _ec2Clients.Clear();

        foreach (var client in _autoScalingClients.Values)
        {
            client.Dispose();
        }

        _autoScalingClients.Clear();

        foreach (var client in _elbv2Clients.Values)
        {
            client.Dispose();
        }

        _elbv2Clients.Clear();

        if (_stsClient.IsValueCreated)
        {
            _stsClient.Value.Dispose();
        }
    }
}
