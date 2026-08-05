using Amazon.EC2;
using Amazon.EC2.Model;
using TruthApi.Models.Aws.Networking;
using TruthApi.Services.Aws.Networking.Infrastructure;

namespace TruthApi.Services.Aws.Networking.Discoverers.Connectivity;

/// <summary>
/// Discovers all Internet Gateways in one AWS Region.
/// </summary>
public sealed class InternetGatewayDiscoverer
{
    public async Task<IReadOnlyList<AwsInternetGatewayInfo>> DiscoverAsync(
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
                var tags = AwsTagHelper.ToDictionary(gateway.Tags);

                results.Add(new AwsInternetGatewayInfo
                {
                    InternetGatewayId =
                        gateway.InternetGatewayId ?? "",
                    Name = AwsTagHelper.GetName(tags),
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
}
