using Amazon.EC2.Model;

namespace TruthApi.Services.Aws.Networking.Infrastructure;

/// <summary>
/// Provides consistent AWS tag conversion and Name-tag extraction.
/// </summary>
public static class AwsTagHelper
{
    public static IReadOnlyDictionary<string, string> ToDictionary(
        IEnumerable<Tag>? tags)
    {
        if (tags is null)
        {
            return new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase);
        }

        return tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag.Key))
            .GroupBy(
                tag => tag.Key,
                StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.Last().Value ?? "",
                StringComparer.OrdinalIgnoreCase);
    }

    public static string GetName(
        IReadOnlyDictionary<string, string> tags)
    {
        return tags.TryGetValue("Name", out var name)
            ? name
            : "";
    }
}
