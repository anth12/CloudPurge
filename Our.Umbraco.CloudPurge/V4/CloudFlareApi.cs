using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Models;
using Newtonsoft.Json;
using Our.Umbraco.CloudPurge.Config;

namespace Our.Umbraco.CloudPurge.V4
{
	internal class CloudFlareApi : ICloudFlareApi
	{
		private readonly IConfigService _configService;
		private readonly HttpClient _httpClient;

		private const string Endpoint = "https://api.cloudflare.com/client/v4";
		private const int MaxRequestSize = 30;

		public CloudFlareApi(IConfigService configService, HttpClient httpClient)
		{
			_configService = configService;
			_httpClient = httpClient;
		}
		
		public async Task<PurgeResponse> PurgeAsync(PurgeRequest request)
		{
			var config = _configService.GetConfig();

			var batchCounter = 0;
			var urlBatches = request.Urls.GroupBy(u => batchCounter++ / MaxRequestSize).ToArray();

			var tasks = urlBatches.Select(urls =>
			{
				var apiRequest = new PurgeCacheRequest(urls, request.Everything);

				var uri = new Uri($"{Endpoint}/zones/{config.ZoneId}/purge_cache");
				return FetchAsync<CloudFlareResponse<PurgeCacheResult>, PurgeCacheRequest>(uri, HttpMethod.Post, apiRequest);
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

					if(response.Messages?.Any() ?? false)
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

		public async Task<bool> ZoneDetailsAsync(string zoneId)
		{

			var uri = new Uri($"{Endpoint}/zones/{zoneId}");
			var (result, exception) = await FetchAsync<CloudFlareResponse<ZoneDetailsResult>, PurgeCacheRequest>(uri, HttpMethod.Get);

			if (exception != null)
				throw exception;

			return result.Result.Paused;			
		}

		private async Task<(TResponse, Exception)> FetchAsync<TResponse, TRequest>(Uri uri, HttpMethod method, TRequest request = default)
		{
			var config = _configService.GetConfig();

			var httpRequest = new HttpRequestMessage(method, uri);

			if (request != null)
			{
				var json = JsonConvert.SerializeObject(request);
				httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
			}

			httpRequest.Headers.Add("X-Auth-Key", config.Token);
			httpRequest.Headers.Add("X-Auth-Email", config.EmailAddress);

			var httpResponse = await _httpClient.SendAsync(httpRequest);

			try
			{
				var responseContent = await httpResponse.Content.ReadAsStringAsync();
				var response = JsonConvert.DeserializeObject<TResponse>(responseContent);

				return (response, null);
			}
			catch (Exception ex)
			{
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
