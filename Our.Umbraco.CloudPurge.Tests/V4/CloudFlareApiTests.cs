using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Models;
using Our.Umbraco.CloudPurge.V4;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using Our.Umbraco.CloudPurge.Config;

namespace Our.Umbraco.CloudPurge.Tests.V4
{
	public class CloudFlareApiTests
	{
		private class MockProvider
		{
			public readonly CloudFlareConfig Settings = new CloudFlareConfig(true, "mock@example.co.uk", "mock-token", "mock-zone-id");
			public readonly Mock<ICloudFlareConfigFactory> CloudFlareConfigFactoryMock = new Mock<ICloudFlareConfigFactory>();
			public readonly Mock<HttpMessageHandler> HttpMessageHandlerMock = new Mock<HttpMessageHandler>();
			public HttpClient HttpClient;

			public ICloudFlareApi GetInstance()
			{
				CloudFlareConfigFactoryMock.Setup(f => f.GetSettings()).Returns(Settings);
				HttpClient = new HttpClient(HttpMessageHandlerMock.Object);
				return new CloudFlareApi(CloudFlareConfigFactoryMock.Object, HttpClient);
			}
		}

		[Test]
		public async Task PurgeAsync_GivenSuccess_WithSingleBatch_ThenExpectedResult()
		{
			var mockProvider = new MockProvider();

			MockHttpRequest(mockProvider, "https://api.cloudflare.com/client/v4/zones/mock-zone-id/purge_cache", HttpMethod.Post,
				(PurgeCacheRequest r) => r.Files.Count() == 2,
				HttpStatusCode.OK, "{ 'success': true }");

			var request = new PurgeRequest(new[] { "mock-url-1", "mock-url-2" }, false);
			using (var instance = mockProvider.GetInstance())
			{
				var result = await instance.PurgeAsync(request);

				Assert.IsTrue(result.Success);
				Assert.IsEmpty(result.FailMessages);
				Assert.IsEmpty(result.FailedUrls);
				Assert.IsNull(result.Exception);
			}

			mockProvider.HttpMessageHandlerMock.VerifyAll();
		}

		[Test]
		public async Task PurgeAsync_GivenSuccess_WithMultipleBatches_ThenExpectedResult()
		{
			var mockProvider = new MockProvider();

			var mockUrlsBatch1 = Enumerable.Range(1, 30).Select(i => "mock-url-" + i);
			var mockUrlsBatch2 = Enumerable.Range(31, 10).Select(i => "mock-url-" + i);

			MockHttpRequest(mockProvider, "https://api.cloudflare.com/client/v4/zones/mock-zone-id/purge_cache", HttpMethod.Post,
				(PurgeCacheRequest r) => r.Files.Count() == 30,
				HttpStatusCode.OK, "{ 'success': true }");

			MockHttpRequest(mockProvider, "https://api.cloudflare.com/client/v4/zones/mock-zone-id/purge_cache", HttpMethod.Post,
				(PurgeCacheRequest r) => r.Files.Count() == 10,
				HttpStatusCode.OK, "{ 'success': true }");

			var request = new PurgeRequest(mockUrlsBatch1.Union(mockUrlsBatch2), false);
			using (var instance = mockProvider.GetInstance())
			{
				var result = await instance.PurgeAsync(request);

				Assert.IsTrue(result.Success);
				Assert.IsEmpty(result.FailMessages);
				Assert.IsEmpty(result.FailedUrls);
				Assert.IsNull(result.Exception);
			}

			mockProvider.HttpMessageHandlerMock
				.Protected().Verify<Task<HttpResponseMessage>>("SendAsync",
					Times.Exactly(2),
					ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()
				);
		}

		[Test]
		public async Task PurgeAsync_GivenFailResponse_WithMultipleBatches_ThenAggregateResponse()
		{
			var mockProvider = new MockProvider();

			var mockUrlsBatch1 = Enumerable.Range(1, 30).Select(i => "mock-url-" + i);
			var mockUrlsBatch2 = Enumerable.Range(31, 10).Select(i => "mock-url-" + i);

			MockHttpRequest(mockProvider, "https://api.cloudflare.com/client/v4/zones/mock-zone-id/purge_cache", HttpMethod.Post,
				(PurgeCacheRequest r) => r.Files.Count() == 30,
				HttpStatusCode.OK, "{ 'success': true }");

			MockHttpRequest(mockProvider, "https://api.cloudflare.com/client/v4/zones/mock-zone-id/purge_cache", HttpMethod.Post,
				(PurgeCacheRequest r) => r.Files.Count() == 10,
				HttpStatusCode.InternalServerError, "{ 'messages': [ 'mock-message' ], 'errors': [ { 'code': 1, 'message': 'mock-error-message'} ] }");

			var request = new PurgeRequest(mockUrlsBatch1.Union(mockUrlsBatch2), false);
			using (var instance = mockProvider.GetInstance())
			{
				var result = await instance.PurgeAsync(request);

				Assert.IsFalse(result.Success);
				Assert.AreEqual(2, result.FailMessages.Count());
				Assert.AreEqual("mock-error-message", result.FailMessages.ElementAt(0));
				Assert.AreEqual("mock-message", result.FailMessages.ElementAt(1));
				Assert.AreEqual(mockUrlsBatch2, result.FailedUrls);
				Assert.IsNull(result.Exception);
			}

			mockProvider.HttpMessageHandlerMock
				.Protected().Verify<Task<HttpResponseMessage>>("SendAsync",
					Times.Exactly(2),
					ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()
				);
		}

		[TestCase(HttpStatusCode.OK)]
		[TestCase(HttpStatusCode.BadRequest)]
		public async Task PurgeAsync_GivenInvalidResponse_ThenReturnException(HttpStatusCode responseStatusCode)
		{
			var mockProvider = new MockProvider();

			var mockUrlsBatch = Enumerable.Range(1, 30).Select(i => "mock-url-" + i);

			MockHttpRequest(mockProvider, "https://api.cloudflare.com/client/v4/zones/mock-zone-id/purge_cache", HttpMethod.Post,
				(PurgeCacheRequest r) => r.Files.Count() == 30,
				responseStatusCode, "--invalid-response--");

			var request = new PurgeRequest(mockUrlsBatch, false);
			using (var instance = mockProvider.GetInstance())
			{
				var result = await instance.PurgeAsync(request);

				Assert.IsFalse(result.Success);
				Assert.IsFalse(result.FailMessages.Any());
				Assert.AreEqual(mockUrlsBatch, result.FailedUrls);
				Assert.IsNotNull(result.Exception);
				Assert.AreEqual(1, result.Exception.InnerExceptions.Count);
			}

			mockProvider.HttpMessageHandlerMock
				.Protected().Verify<Task<HttpResponseMessage>>("SendAsync",
					Times.Once(),
					ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()
				);
		}

		private static void MockHttpRequest<TRequest>(MockProvider mockProvider, string uri, HttpMethod method, Func<TRequest, bool> verifyRequest, HttpStatusCode responseStatus, string responseBody)
		{
			mockProvider.HttpMessageHandlerMock
				.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
					ItExpr.Is<HttpRequestMessage>(r => r.RequestUri.ToString() == uri &&
													   r.Method == method &&
													   r.Headers.GetValues("X-Auth-Key").SingleOrDefault() == mockProvider.Settings.Token &&
													   r.Headers.GetValues("X-Auth-Email").SingleOrDefault() == mockProvider.Settings.EmailAddress &&
													   verifyRequest.Invoke(JsonConvert.DeserializeObject<TRequest>(r.Content.ReadAsStringAsync().Result))
					), ItExpr.IsAny<CancellationToken>()
				).ReturnsAsync(new HttpResponseMessage
				{
					StatusCode = responseStatus,
					Content = new StringContent(responseBody),
				}).Verifiable();
		}

	}
}
