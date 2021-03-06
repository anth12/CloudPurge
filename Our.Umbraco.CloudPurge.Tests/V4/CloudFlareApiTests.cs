﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Models;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using Our.Umbraco.CloudPurge.Cdn;
using Our.Umbraco.CloudPurge.Cdn.CloudFlare;
using Our.Umbraco.CloudPurge.CDN.CloudFlare;
using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.Domain;
using Umbraco.Core.Logging;

namespace Our.Umbraco.CloudPurge.Tests.V4
{
	public class CloudFlareApiTests
	{
		private class MockProvider
		{
			public CloudPurgeConfig Config { get; set; } = new CloudPurgeConfig(true, null, new CloudFlareConfig(true, "mock@example.co.uk", "mock-token", "mock-zone-id"));
			public readonly Mock<IConfigService> CloudFlareConfigFactoryMock = new Mock<IConfigService>();
			public readonly Mock<HttpMessageHandler> HttpMessageHandlerMock = new Mock<HttpMessageHandler>();
			public readonly Mock<ILogger> LoggerMock = new Mock<ILogger>();
			public HttpClient HttpClient;

			public ICdnApi GetInstance()
			{
				CloudFlareConfigFactoryMock.Setup(f => f.GetConfig()).Returns(Config);
				HttpClient = new HttpClient(HttpMessageHandlerMock.Object);
				return new CloudFlareV4Api(CloudFlareConfigFactoryMock.Object, HttpClient, LoggerMock.Object);
			}
		}

		[TestCase(true, true)]
		[TestCase(false, false)]
		public void IsEnabled_GivenConfigValue_ThenReturnsConfig(bool configMock, bool expected)
		{
			var mockProvider = new MockProvider
			{
				Config = new CloudPurgeConfig(true, null, new CloudFlareConfig(configMock, null, null, null))
			};

			using (var instance = mockProvider.GetInstance())
			{
				var result = instance.IsEnabled();

				Assert.AreEqual(expected, result);
			}
		}

		[Test]
		public async Task PurgeByUrlAsync_GivenSuccess_ThenExpectedResult()
		{
			var mockProvider = new MockProvider();

			MockHttpRequest(mockProvider, "https://api.cloudflare.com/client/v4/zones/mock-zone-id/purge_cache", HttpMethod.Post,
				(PurgeFilesCacheRequest r) => r.Files.Count() == 2,
				HttpStatusCode.OK, "{ 'success': true }");

			var request = new PurgeRequest(new[] { "mock-url-1", "mock-url-2" });
			using (var instance = mockProvider.GetInstance())
			{
				var result = await instance.PurgeByUrlAsync(request);

				Assert.AreEqual(PurgeResult.Success, result.Result);
				Assert.IsEmpty(result.FailMessages);
				Assert.IsEmpty(result.FailedUrls);
				Assert.IsNull(result.Exception);
			}

			mockProvider.HttpMessageHandlerMock.VerifyAll();
		}

		[Test]
		public async Task PurgeByUrlAsync_GivenUnsuccessful_ThenExpectedResult()
		{
			var mockProvider = new MockProvider();

			MockHttpRequest(mockProvider, "https://api.cloudflare.com/client/v4/zones/mock-zone-id/purge_cache", HttpMethod.Post,
				(PurgeFilesCacheRequest r) => r.Files.Count() == 2,
				HttpStatusCode.OK, "{ 'success': false, 'Messages': ['mock-message'], 'Errors': [{ 'Message': 'mock-error'}] }");

			var request = new PurgeRequest(new[] { "mock-url-1", "mock-url-2" });
			using (var instance = mockProvider.GetInstance())
			{
				var result = await instance.PurgeByUrlAsync(request);

				Assert.AreEqual(PurgeResult.Fail, result.Result);
				Assert.AreEqual(2, result.FailMessages.Count());
				Assert.AreEqual("mock-message", result.FailMessages.ElementAt(0));
				Assert.AreEqual("mock-error", result.FailMessages.ElementAt(1));
				Assert.IsEmpty(result.FailedUrls);
				Assert.IsNull(result.Exception);
			}

			mockProvider.HttpMessageHandlerMock.VerifyAll();
		}

		[TestCase(HttpStatusCode.OK)]
		[TestCase(HttpStatusCode.BadRequest)]
		public async Task PurgeByUrlAsync_GivenInvalidResponse_ThenReturnException(HttpStatusCode responseStatusCode)
		{
			var mockProvider = new MockProvider();

			var mockUrlsBatch = Enumerable.Range(1, 30).Select(i => "mock-url-" + i);

			MockHttpRequest(mockProvider, "https://api.cloudflare.com/client/v4/zones/mock-zone-id/purge_cache", HttpMethod.Post,
				(PurgeFilesCacheRequest r) => r.Files.Count() == 30,
				responseStatusCode, "--invalid-response--");

			var request = new PurgeRequest(mockUrlsBatch);
			using (var instance = mockProvider.GetInstance())
			{
				var result = await instance.PurgeByUrlAsync(request);

				Assert.AreEqual(PurgeResult.Fail, result.Result);
				Assert.IsFalse(result.FailMessages.Any());
				Assert.AreEqual(mockUrlsBatch, result.FailedUrls[CdnType.CloudFlare]);
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
													   r.Headers.GetValues("X-Auth-Key").SingleOrDefault() == mockProvider.Config.CloudFlare.Token &&
													   r.Headers.GetValues("X-Auth-Email").SingleOrDefault() == mockProvider.Config.CloudFlare.EmailAddress &&
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
