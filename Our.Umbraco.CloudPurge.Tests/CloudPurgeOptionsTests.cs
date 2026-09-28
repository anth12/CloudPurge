using NUnit.Framework;
using Our.Umbraco.CloudPurge;

namespace Our.Umbraco.CloudPurge.Tests;

public sealed class CloudPurgeOptionsTests
{
    [Test]
    public void FiltersUseAliasesAndExclusionsWin()
    {
        var options = new CloudPurgeOptions
        {
            IncludedContentTypes = "article, landingPage",
            ExcludedContentTypes = "landingPage"
        };

        Assert.Multiple(() =>
        {
            Assert.That(options.Allows("article"), Is.True);
            Assert.That(options.Allows("landingPage"), Is.False);
            Assert.That(options.Allows("other"), Is.False);
        });
    }
}
