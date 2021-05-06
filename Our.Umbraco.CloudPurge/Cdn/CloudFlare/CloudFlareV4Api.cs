using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Our.Umbraco.CloudPurge.Cdn;
using Our.Umbraco.CloudPurge.Cdn.CloudFlare;
using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.Domain;
using Our.Umbraco.CloudPurge.Models;

namespace Our.Umbraco.CloudPurge.CDN.CloudFlare
{
	internal class CloudFlareV4Api : ICdnApi
	{
		private readonly ILogger<CloudFlareV4Api> _logger;
		private readonly IConfigService _configService;
		private readonly HttpClient _httpClient;

		private const string Endpoint = "https://api.cloudflare.com/client/v4";
		private const int MaxRequestSize = 30;

		public CloudFlareV4Api(IConfigService configService, HttpClient httpClient, ILogger<CloudFlareV4Api> logger)
		{
			_configService = configService;
			_httpClient = httpClient;
			_logger = logger;
		}

		public int MaxBatchSize => MaxRequestSize;
		public bool IsEnabled() => _configService.GetConfig().CloudFlare.Enabled;

		public async Task<PurgeResponse> PurgeByUrlAsync(PurgeRequest request)
		{
			var config = _configService.GetConfig();

			var apiRequest = new PurgeFilesCacheRequest(request.Urls);

			var uri = new Uri($"{Endpoint}/zones/{config.CloudFlare.ZoneId}/purge_cache");

			try
			{
				var result = await FetchAsync<CloudFlareResponse<PurgeCacheResult>, PurgeFilesCacheRequest>(uri, HttpMethod.Post, apiRequest);

				return new PurgeResponse(
					result: result.Success ? PurgeResult.Success : PurgeResult.Fail,
					cdnType: CdnType.CloudFlare,
					failedUrls: null, // TODO
					failMessages: (result.Messages ?? Array.Empty<string>()).Union(result.Errors?.Select(e=> e.Message) ?? Array.Empty<string>()),
					exception: null);
			}
			catch (Exception ex)
			{
				return new PurgeResponse(
					result: PurgeResult.Fail,
					cdnType: CdnType.CloudFlare,
					failedUrls: request.Urls,
					failMessages: null,
					exception: new AggregateException(ex));
			}
		}

		public async Task<PurgeResponse> PurgeAllAsync(PurgeAllRequest request)
		{
			var config = _configService.GetConfig();

			var apiRequest = new PurgeAllCacheRequest(true);

			var uri = new Uri($"{Endpoint}/zones/{config.CloudFlare.ZoneId}/purge_cache");
			try
			{
				var result = await FetchAsync<CloudFlareResponse<PurgeCacheResult>, PurgeAllCacheRequest>(uri, HttpMethod.Post, apiRequest);

				return new PurgeResponse(
					result: result.Success ? PurgeResult.Success : PurgeResult.Fail,
					failedUrls: null,
					failMessages: (result.Messages ?? Array.Empty<string>()).Union(result.Errors?.Select(e => e.Message) ?? Array.Empty<string>()),
					exception: null);
			}
			catch (Exception ex)
			{
				return new PurgeResponse(
					result: PurgeResult.Fail,
					failedUrls: null,
					failMessages: null,
					exception: new AggregateException(ex));
			}
		}
		
		public async Task<bool> HealthCheckAsync()
		{
			var config = _configService.GetConfig();

			var uri = new Uri($"{Endpoint}/zones/{config.CloudFlare.ZoneId}");

			try
			{
				var result = await FetchAsync<CloudFlareResponse<ZoneDetailsResult>, PurgeFilesCacheRequest>(uri, HttpMethod.Get);
				return result.Success;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		private async Task<TResponse> FetchAsync<TResponse, TRequest>(Uri uri, HttpMethod method, TRequest request = default)
		{
			var config = _configService.GetConfig();

			var httpRequest = new HttpRequestMessage(method, uri);

			if (request != null)
			{
				var json = JsonConvert.SerializeObject(request);
				_logger.LogTrace("Sending {RequestType} with payload {payload}", request.GetType().Name, json);

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
				_logger.LogError(ex, "Failed to send request");

				throw;
			}

			try
			{
				var responseContent = await httpResponse.Content.ReadAsStringAsync();
				_logger.LogTrace("Deserializing response {ResponseType} from {Response} ", typeof(TResponse).Name, responseContent);

				var response = JsonConvert.DeserializeObject<TResponse>(responseContent);

				return response;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Unable to deserialise CloudFlare. ResponseCode {responseCode}", httpResponse.StatusCode);

				if (httpResponse.IsSuccessStatusCode)
					throw new Exception($"Unsuccessful response code {httpResponse.StatusCode}");

				throw new SerializationException($"Unable to deserialise CloudFlare response to {typeof(TResponse)}", ex);
			}
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}
}
