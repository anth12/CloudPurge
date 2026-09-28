using System.Net;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Our.Umbraco.CloudPurge;

namespace Our.Umbraco.CloudPurge.Tests;

public sealed class CloudflareClientTests
{
    [Test]
    public async Task PurgeUsesScopedBearerTokenAndDeduplicatesUrls()
    {
        var calls = new List<(Uri Uri, string? Authorization, string Body)>();
        using var http = new HttpClient(new Handler(async request =>
        {
            calls.Add((request.RequestUri!, request.Headers.Authorization?.ToString(),
                await request.Content!.ReadAsStringAsync()));
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"success\":true}") };
        }));
        var client = CreateClient(http);

        var count = await client.PurgeAsync(new[] { "https://example.com/a", "https://example.com/a", "/relative" });

        Assert.That(count, Is.EqualTo(1));
        Assert.That(calls, Has.Count.EqualTo(1));
        Assert.That(calls[0].Uri.AbsoluteUri, Is.EqualTo("https://api.cloudflare.com/client/v4/zones/zone/purge_cache"));
        Assert.That(calls[0].Authorization, Is.EqualTo("Bearer secret"));
        Assert.That(calls[0].Body, Does.Contain("https://example.com/a"));
    }

    [Test]
    public void PurgeRejectsCloudflareFailure()
    {
        using var http = new HttpClient(new Handler(_ => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"success\":false}") })));

        Assert.ThrowsAsync<HttpRequestException>(async () =>
            await CreateClient(http).PurgeAsync(new[] { "https://example.com/a" }));
    }

    [Test]
    public async Task PurgeSplitsLargeSetsIntoBatches()
    {
        var calls = 0;
        using var http = new HttpClient(new Handler(_ =>
        {
            calls++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent("{\"success\":true}") });
        }));

        var count = await CreateClient(http).PurgeAsync(
            Enumerable.Range(0, 201).Select(i => $"https://example.com/{i}"));

        Assert.That(count, Is.EqualTo(201));
        Assert.That(calls, Is.EqualTo(3));
    }

    [Test]
    public async Task PurgeRetriesRateLimits()
    {
        var calls = 0;
        using var http = new HttpClient(new Handler(_ =>
        {
            calls++;
            return Task.FromResult(calls == 1
                ? new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                {
                    Headers = { RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.Zero) }
                }
                : new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"success\":true}")
                });
        }));

        Assert.That(await CreateClient(http).PurgeAsync(new[] { "https://example.com/a" }), Is.EqualTo(1));
        Assert.That(calls, Is.EqualTo(2));
    }

    private static CloudflareClient CreateClient(HttpClient http) => new(http,
        Options.Create(new CloudPurgeOptions { Cloudflare = new CloudflareOptions { ApiToken = "secret", ZoneId = "zone" } }));

    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> handle) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => handle(request);
    }
}
