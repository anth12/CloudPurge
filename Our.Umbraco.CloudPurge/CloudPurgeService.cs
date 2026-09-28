using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Our.Umbraco.CloudPurge;

public sealed class CloudPurgeService(
    IUmbracoContextFactory contextFactory,
    CloudflareClient cloudflare,
    IOptions<CloudPurgeOptions> options,
    ILogger<CloudPurgeService> logger)
{
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
        var cultures = item.Cultures.Count == 0 ? new string?[] { null } : item.Cultures.Keys.Cast<string?>();
        foreach (var culture in cultures)
        {
            var url = item.Url(culture: culture, mode: UrlMode.Absolute);
            if (Uri.TryCreate(url, UriKind.Absolute, out _))
                yield return url;
        }
    }
}
