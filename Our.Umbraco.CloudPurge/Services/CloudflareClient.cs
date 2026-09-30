using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Our.Umbraco.CloudPurge;

public sealed class CloudflareClient(HttpClient httpClient, IOptions<CloudPurgeOptions> options,
    ILogger<CloudflareClient> logger)
{
    private const int BatchSize = 100;
    private readonly CloudPurgeOptions _options = options.Value;

    public async Task<int> PurgeAsync(IEnumerable<string> urls, CancellationToken cancellationToken = default)
    {
        var distinct = urls.Where(url => Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        foreach (var batch in distinct.Chunk(BatchSize))
        {
            await PurgeBatchAsync(batch, cancellationToken);
        }

        return distinct.Length;
    }

    private async Task PurgeBatchAsync(string[] batch, CancellationToken cancellationToken)
    {
        try
        {
            for (var attempt = 0; attempt < 3; attempt++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Post,
                    $"https://api.cloudflare.com/client/v4/zones/{Uri.EscapeDataString(_options.Cloudflare.ZoneId)}/purge_cache");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Cloudflare.ApiToken);
                request.Content = JsonContent.Create(new { files = batch });

                using var response = await httpClient.SendAsync(request, cancellationToken);
                if (response.StatusCode == HttpStatusCode.TooManyRequests && attempt < 2)
                {
                    var retryAfter = response.Headers.RetryAfter;
                    var delay = retryAfter?.Delta
                        ?? (retryAfter?.Date - DateTimeOffset.UtcNow)
                        ?? TimeSpan.FromSeconds(1 << attempt);
                    delay = TimeSpan.FromTicks(Math.Clamp(delay.Ticks, 0, TimeSpan.FromSeconds(30).Ticks));
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                CloudflarePurgeResponse? result;
                try
                {
                    result = JsonSerializer.Deserialize<CloudflarePurgeResponse>(body);
                }
                catch (JsonException exception) when (response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Cloudflare returned an invalid purge response.", exception);
                }
                catch (JsonException)
                {
                    result = null;
                }

                if (!response.IsSuccessStatusCode || result?.Success != true)
                {
                    var errors = result?.Errors?.Select(error => $"{error.Code}: {error.Message}").ToArray() ?? [];
                    var reason = errors.Length == 0
                        ? "Cloudflare did not accept the purge request"
                        : string.Join("; ", errors);
                    throw new HttpRequestException(
                        $"Cloudflare purge failed with HTTP {(int)response.StatusCode} ({response.ReasonPhrase}): {reason}.",
                        null, response.StatusCode);
                }

                return;
            }
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(exception, "Cloudflare purge failed for URLs: {Urls}. Reason: {Reason}",
                string.Join(", ", batch), exception.Message);
            throw;
        }
    }

    private sealed class CloudflarePurgeResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("errors")]
        public CloudflareError[]? Errors { get; set; }
    }

    private sealed class CloudflareError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }
}
