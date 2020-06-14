using System.Linq;
using NUnit.Framework;
using Our.Umbraco.CloudPurge.Domain;
using Our.Umbraco.CloudPurge.Models;

namespace Our.Umbraco.CloudPurge.Tests.Models
{
	public class PurgeResponseTests
	{
		[Test]
		public void Aggregate_GivenNoExceptions_ThenNullException()
		{
			var subjects = new[]
			{
				new PurgeResponse(true, null, null, null),
				new PurgeResponse(true, null, null, null)
			};

			var result = PurgeResponse.Aggregate(subjects);

			Assert.IsNull(result.Exception);

			// Default assertions
			Assert.AreEqual(true, result.Success);
			Assert.AreEqual(0, result.FailedUrls.Count());
			Assert.AreEqual(0, result.FailMessages.Count());
		}

		[Test]
		public void Aggregate_GivenPartialSuccess_ThenNotSuccessful()
		{
			var subjects = new[]
			{
				new PurgeResponse(false, null, null, null),
				new PurgeResponse(true, null, null, null)
			};

			var result = PurgeResponse.Aggregate(subjects);

			Assert.AreEqual(false, result.Success);

			// Default assertions
			Assert.AreEqual(0, result.FailedUrls.Count());
			Assert.AreEqual(0, result.FailMessages.Count());
			Assert.IsNull(result.Exception);
		}

		[Test]
		public void Aggregate_GivenFailedUrls_ThenAggregate()
		{
			var subjects = new[]
			{
				new PurgeResponse(false, CdnType.CloudFlare,new []{ "mock/1", "mock/2"}, null, null),
				new PurgeResponse(true, CdnType.CloudFlare,null, null, null),
				new PurgeResponse(false, CdnType.CloudFlare,new []{ "mock/3" }, null, null)
			};

			var result = PurgeResponse.Aggregate(subjects);

			Assert.AreEqual(1, result.FailedUrls.Count());
			Assert.AreEqual(3, result.FailedUrls[CdnType.CloudFlare].Count());

			// Default assertions
			Assert.AreEqual(false, result.Success);
			Assert.AreEqual(0, result.FailMessages.Count());
			Assert.IsNull(result.Exception);
		}

		[Test]
		public void Aggregate_GivenFailMessages_ThenAggregate()
		{
			var subjects = new[]
			{
				new PurgeResponse(false, null, new []{ "mock 1", "mock 2"}, null),
				new PurgeResponse(true, null, null, null),
				new PurgeResponse(false, null, new []{ "mock 3" }, null)
			};

			var result = PurgeResponse.Aggregate(subjects);

			Assert.AreEqual(3, result.FailMessages.Count());

			// Default assertions
			Assert.AreEqual(false, result.Success);
			Assert.AreEqual(0, result.FailedUrls.Count());
			Assert.IsNull(result.Exception);
		}
	}
}
