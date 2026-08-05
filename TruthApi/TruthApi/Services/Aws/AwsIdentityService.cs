using Amazon.SecurityToken.Model;
using TruthApi.Models.Aws;

namespace TruthApi.Services.Aws;

/// <summary>
/// Discovers the AWS account and principal used by TruthApi.
/// </summary>
public sealed class AwsIdentityService
{
    private readonly AwsClientFactory _clientFactory;

    public AwsIdentityService(AwsClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<AwsIdentityInfo> GetCurrentIdentityAsync(
        CancellationToken cancellationToken = default)
    {
        var client = _clientFactory.GetStsClient();

        var response = await client.GetCallerIdentityAsync(
            new GetCallerIdentityRequest(),
            cancellationToken);

        var arn = response.Arn ?? "";

        return new AwsIdentityInfo
        {
            AccountId = response.Account ?? "",
            Arn = arn,
            PrincipalId = response.UserId ?? "",
            DisplayName = ExtractDisplayName(arn),
            DiscoveredAtUtc = DateTime.UtcNow
        };
    }

    private static string ExtractDisplayName(string arn)
    {
        if (string.IsNullOrWhiteSpace(arn))
        {
            return "";
        }

        var slashIndex = arn.LastIndexOf('/');

        if (slashIndex >= 0 && slashIndex < arn.Length - 1)
        {
            return arn[(slashIndex + 1)..];
        }

        var colonIndex = arn.LastIndexOf(':');

        return colonIndex >= 0 && colonIndex < arn.Length - 1
            ? arn[(colonIndex + 1)..]
            : arn;
    }
}
