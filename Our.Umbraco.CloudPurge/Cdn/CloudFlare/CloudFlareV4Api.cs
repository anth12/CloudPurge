using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Our.Umbraco.CloudPurge.Cdn;
using Our.Umbraco.CloudPurge.Cdn.CloudFlare;
using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.Models;
using Serilog;

namespace Our.Umbraco.CloudPurge.CDN.CloudFlare
{
	internal class CloudFlareV4Api : ICdnApi
	{
		private readonly ILogger _logger;
		private readonly IConfigService _configService;
		private readonly HttpClient _httpClient;

		private const string Endpoint = "https://api.cloudflare.com/client/v4";
		private const int MaxRequestSize = 30;

		public CloudFlareV4Api(IConfigService configService, HttpClient httpClient)
		{
			_logger = Log.ForContext<CloudFlareV4Api>();
			_configService = configService;
			_httpClient = httpClient;
		}

		public bool IsEnabled()
			=> _configService.GetConfig().CloudFlare.Enabled;

		public Task<PurgeResponse> PurgeAsync(PurgeRequest request)
		{
			return request.Everything 
				? PurgeEverythingAsync() 
				: PurgeUrlsAsync(request);
		}

		private async Task<PurgeResponse> PurgeEverythingAsync()
		{
			var config = _configService.GetConfig();

			var apiRequest = new PurgeAllCacheRequest(true);

			var uri = new Uri($"{Endpoint}/zones/{config.CloudFlare.ZoneId}/purge_cache");
			var (result, error) = await FetchAsync<CloudFlareResponse<PurgeCacheResult>, PurgeAllCacheRequest>(uri, HttpMethod.Post, apiRequest);

			return new PurgeResponse(
				success: result?.Success ?? false,
				failedUrls: null,
				failMessages: result?.Messages,
				exception: error != null ? new AggregateException(error) : null);
		}

		private async Task<PurgeResponse> PurgeUrlsAsync(PurgeRequest request)
		{
			var config = _configService.GetConfig();

			var batchCounter = 0;
			var urlBatches = request.Urls.GroupBy(u => batchCounter++ / MaxRequestSize).ToArray();

			_logger.Debug("Sending purge request in {BatchCount} batches", urlBatches.Length);

			var tasks = urlBatches.Select(urls =>
			{
				var apiRequest = new PurgeFilesCacheRequest(urls);

				var uri = new Uri($"{Endpoint}/zones/{config.CloudFlare.ZoneId}/purge_cache");
				return FetchAsync<CloudFlareResponse<PurgeCacheResult>, PurgeFilesCacheRequest>(uri, HttpMethod.Post,
					apiRequest);
			});

			var responses = await Task.WhenAll(tasks);

			var errors = new List<Exception>();
			var failedUrls = new List<string>();
			var failMessages = new List<string>();

			var batch = 0;
			foreach (var (response, error) in responses)
			{
				if (error != null)
				{
					errors.Add(error);
					failedUrls.AddRange(urlBatches[batch]);
				}
				else if (!response.Success)
				{
					failedUrls.AddRange(urlBatches[batch]);
					failMessages.AddRange(response.Errors?.Select(e => e.Message) ?? Array.Empty<string>());

					if (response.Messages?.Any() ?? false)
						failMessages.AddRange(response.Messages);
				}

				batch++;
			}

			return new PurgeResponse(
				success: !failedUrls.Any(),
				failedUrls: failedUrls,
				failMessages: failMessages,
				exception: errors.Any() ? new AggregateException(errors) : null
			);
		}

		public async Task<bool> HealthCheckAsync()
		{
			var config = _configService.GetConfig();

			var uri = new Uri($"{Endpoint}/zones/{config.CloudFlare.ZoneId}");
			var (result, exception) = await FetchAsync<CloudFlareResponse<ZoneDetailsResult>, PurgeFilesCacheRequest>(uri, HttpMethod.Get);

			if (exception != null)
				throw exception;

			return !string.IsNullOrEmpty(result.Result?.Id);
		}

		private async Task<(TResponse, Exception)> FetchAsync<TResponse, TRequest>(Uri uri, HttpMethod method, TRequest request = default)
		{
			var config = _configService.GetConfig();

			var httpRequest = new HttpRequestMessage(method, uri);

			if (request != null)
			{
				var json = JsonConvert.SerializeObject(request);
				_logger.Debug("Sending {RequestType} with payload {payload}", request.GetType().Name, json);

				httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
			}

			httpRequest.Headers.Add("X-Auth-Key", config.CloudFlare.Token);
			httpRequest.Headers.Add("X-Auth-Email", config.CloudFlare.EmailAddress);

			HttpResponseMessage httpResponse;
			try
			{
				httpResponse = await _httpClient.SendAsync(httpRequest);
			}
			catch (Exception ex)
			{
				_logger.Error("Failed to send request", ex);

				return (default, ex);
			}

			try
			{
				var responseContent = await httpResponse.Content.ReadAsStringAsync();
				_logger.Verbose("Deserializing response {ResponseType} from {Response} ", typeof(TResponse).Name, responseContent);

				var response = JsonConvert.DeserializeObject<TResponse>(responseContent);

				return (response, null);
			}
			catch (Exception ex)
			{
				_logger.Error("Error response", ex);

				if (httpResponse.IsSuccessStatusCode)
					return (default, new Exception($"Unsuccessful response code {httpResponse.StatusCode}"));

				return (default, new SerializationException($"Unable to deserialise CloudFlare response to {typeof(TResponse)}", ex));
			}
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}
}
