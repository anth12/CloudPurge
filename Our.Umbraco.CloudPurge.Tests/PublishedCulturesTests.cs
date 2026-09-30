using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Our.Umbraco.CloudPurge.Tests;

public sealed class PublishedCulturesTests
{
    [Test]
    public void VariantContentUsesEachPublishedCulture()
    {
        var content = CreateContent(new Dictionary<string, PublishedCultureInfo>
        {
            ["en-US"] = null!,
            ["fr-FR"] = null!
        });

        Assert.That(CloudPurgeService.GetCultures(content), Is.EquivalentTo(new[] { "en-US", "fr-FR" }));
    }

    [Test]
    public void InvariantContentUsesDefaultCulture()
    {
        var content = CreateContent(new Dictionary<string, PublishedCultureInfo>());

        Assert.That(CloudPurgeService.GetCultures(content), Is.EqualTo(new string?[] { null }));
    }

    private static IPublishedContent CreateContent(IReadOnlyDictionary<string, PublishedCultureInfo> cultures)
    {
        var content = DispatchProxy.Create<IPublishedContent, PublishedContentProxy>();
        ((PublishedContentProxy)content).Cultures = cultures;
        return content;
    }

    public class PublishedContentProxy : DispatchProxy
    {
        public IReadOnlyDictionary<string, PublishedCultureInfo> Cultures { get; set; } =
            new Dictionary<string, PublishedCultureInfo>();

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) =>
            targetMethod?.Name == "get_Cultures" ? Cultures : throw new NotSupportedException(targetMethod?.Name);
    }
}
