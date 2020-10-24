using System.Collections.Generic;
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
				new PurgeResponse(PurgeResult.Success, null, null, null),
				new PurgeResponse(PurgeResult.Success, null, null, null)
			};

			var result = PurgeResponse.Aggregate(subjects);

			Assert.IsNull(result.Exception);

			// Default assertions
			Assert.AreEqual(PurgeResult.Success, result.Result);
			Assert.AreEqual(0, result.FailedUrls.Count());
			Assert.AreEqual(0, result.FailMessages.Count());
		}

		[Test]
		public void Aggregate_GivenPartialSuccess_ThenNotSuccessful()
		{
			var subjects = new[]
			{
				new PurgeResponse(PurgeResult.Success, null, null, null),
				new PurgeResponse(PurgeResult.Fail, null, null, null)
			};

			var result = PurgeResponse.Aggregate(subjects);

			Assert.AreEqual(PurgeResult.Fail, result.Result);

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
				new PurgeResponse(PurgeResult.Fail, CdnType.CloudFlare,new []{ "mock/1", "mock/2"}, null, null),
				new PurgeResponse(PurgeResult.Success, CdnType.CloudFlare,null, null, null),
				new PurgeResponse(PurgeResult.Fail, CdnType.CloudFlare,new []{ "mock/3" }, null, null)
			};

			var result = PurgeResponse.Aggregate(subjects);

			Assert.AreEqual(1, result.FailedUrls.Count());
			Assert.AreEqual(3, result.FailedUrls[CdnType.CloudFlare].Count());

			// Default assertions
			Assert.AreEqual(PurgeResult.Fail, result.Result);
			Assert.AreEqual(0, result.FailMessages.Count());
			Assert.IsNull(result.Exception);
		}

		[Test]
		public void Aggregate_GivenFailMessages_ThenAggregate()
		{
			var subjects = new[]
			{
				new PurgeResponse(PurgeResult.Fail, null, new []{ "mock 1", "mock 2"}, null),
				new PurgeResponse(PurgeResult.Success, null, null, null),
				new PurgeResponse(PurgeResult.Fail, null, new []{ "mock 3" }, null)
			};

			var result = PurgeResponse.Aggregate(subjects);

			Assert.AreEqual(3, result.FailMessages.Count());

			// Default assertions
			Assert.AreEqual(PurgeResult.Fail, result.Result);
			Assert.AreEqual(0, result.FailedUrls.Count());
			Assert.IsNull(result.Exception);
		}

		[TestCaseSource(nameof(MultipleResultsSource))]
		public void Aggregate_GivenMultipleResults_ThenPrioritise(PurgeResult[] mockResults, PurgeResult expectedResult)
		{
			var subjects = mockResults.Select(r => new PurgeResponse(r, null, null, null)).ToArray();

			var result = PurgeResponse.Aggregate(subjects);

			Assert.AreEqual(expectedResult, result.Result);
		}

		public static IEnumerable<object[]> MultipleResultsSource()
        {
			yield return new object[] 
			{ 
				new[] { PurgeResult.Fail, PurgeResult.NothingPurged, PurgeResult.Success }, 
				PurgeResult.Fail 
			};

			yield return new object[]
			{
				new[] { PurgeResult.NothingPurged, PurgeResult.NothingPurged, PurgeResult.Success },
				PurgeResult.NothingPurged
			};

			yield return new object[]
			{
				new[] { PurgeResult.Fail, PurgeResult.Success, PurgeResult.Success },
				PurgeResult.Fail
			};

			yield return new object[]
			{
				new[] { PurgeResult.Success, PurgeResult.Success },
				PurgeResult.Success
			};
		}
	}
}
