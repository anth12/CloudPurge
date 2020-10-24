using System;
using System.Collections.Generic;
using System.Linq;
using Our.Umbraco.CloudPurge.Domain;

namespace Our.Umbraco.CloudPurge.Models
{
	public class PurgeResponse
	{
		public PurgeResponse(PurgeResult result, IDictionary<CdnType, string[]> failedUrls, IEnumerable<string> failMessages, AggregateException exception)
		{
			Result = result;
			FailedUrls = failedUrls ?? new Dictionary<CdnType, string[]>();
			FailMessages = failMessages ?? new string[0];
			Exception = exception;
		}

		public PurgeResponse(PurgeResult result, CdnType cdnType, IEnumerable<string> failedUrls, IEnumerable<string> failMessages, AggregateException exception)
		{
			Result = result;
			FailedUrls = failedUrls?.Any() ?? false ? new Dictionary<CdnType, string[]>
			{
				{ cdnType, failedUrls.ToArray() }
			} : new Dictionary<CdnType, string[]>();
			FailMessages = failMessages ?? new string[0];
			Exception = exception;
		}

		public PurgeResult Result { get; }

		public IDictionary<CdnType, string[]> FailedUrls { get; }
		public IEnumerable<string> FailMessages { get; }
		public AggregateException Exception { get; }
		
		public static PurgeResponse Aggregate(PurgeResponse[] responses)
		{
			var exceptions = responses
				.SelectMany(r => r.Exception?.InnerExceptions.AsEnumerable() ?? new Exception[0])
				.ToArray();

			var result = responses
				.GroupBy(r => r.Result)
				.OrderByDescending(g=> g.Key)
				.FirstOrDefault()
				.Key;

			return new PurgeResponse(
				result: result,
				failedUrls: responses.SelectMany(r=> r.FailedUrls)
					.GroupBy(r=> r.Key)
					.ToDictionary(g=> g.Key, g=> g.SelectMany(r=> r.Value).ToArray()),
				failMessages: responses.SelectMany(r => r.FailMessages),
				exception: exceptions.Any() ? new AggregateException(exceptions) : null
			);
		}
	}
}
