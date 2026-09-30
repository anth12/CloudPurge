using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace Our.Umbraco.CloudPurge;

public sealed class CloudPurgeService(
    IUmbracoContextFactory contextFactory,
    CloudflareClient cloudflare,
    IOptions<CloudPurgeOptions> options,
    ILogger<CloudPurgeService> logger)
{
    // Umbraco 18 moved Cultures from IPublishedContent to IPublishedElement.
    // Resolve the getter at runtime so one package works with both versions.
    private static readonly PropertyInfo CulturesProperty =
        typeof(IPublishedContent).GetProperty("Cultures")
        ?? typeof(IPublishedElement).GetProperty("Cultures")
        ?? throw new NotSupportedException("The installed Umbraco version does not expose published cultures.");

    public async Task<int> PurgeAsync(IEnumerable<int> contentIds, bool descendants, CancellationToken cancellationToken = default)
    {
        return await PurgeUrlsAsync(GetUrls(contentIds, descendants), cancellationToken);
    }

    public string[] GetUrls(IEnumerable<int> contentIds, bool descendants)
    {
        using var context = contextFactory.EnsureUmbracoContext();
        var contentCache = context.UmbracoContext.Content;
        if (contentCache is null)
            return [];

        var content = contentIds.Select(contentCache.GetById).OfType<IPublishedContent>();
        return (descendants ? content.SelectMany(WithDescendants) : content)
            .Where(item => options.Value.Allows(item.ContentType.Alias))
            .SelectMany(GetUrls)
            .ToArray();
    }

    public async Task<int> PurgeUrlsAsync(IEnumerable<string> urls, CancellationToken cancellationToken = default)
    {
        var count = await cloudflare.PurgeAsync(urls, cancellationToken);
        logger.LogInformation("CloudPurge submitted {Count} URLs to Cloudflare", count);
        return count;
    }

    private static IEnumerable<IPublishedContent> WithDescendants(IPublishedContent item)
    {
        yield return item;
        foreach (var child in item.Children())
            foreach (var descendant in WithDescendants(child))
                yield return descendant;
    }

    private static IEnumerable<string> GetUrls(IPublishedContent item)
    {
        foreach (var culture in GetCultures(item))
        {
            var url = item.Url(culture: culture, mode: UrlMode.Absolute);
            if (Uri.TryCreate(url, UriKind.Absolute, out _))
                yield return url;
        }
    }

    internal static IEnumerable<string?> GetCultures(IPublishedContent item)
    {
        var cultures = (IReadOnlyDictionary<string, PublishedCultureInfo>)CulturesProperty.GetValue(item)!;
        return cultures.Count == 0 ? [null] : cultures.Keys.Cast<string?>();
    }
}
