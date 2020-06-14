using System;
using System.Collections.Generic;
using System.Linq;
using Our.Umbraco.CloudPurge.Domain;

namespace Our.Umbraco.CloudPurge.Models
{
	public class PurgeResponse
	{
		public PurgeResponse(bool success, IDictionary<CdnType, string[]> failedUrls, IEnumerable<string> failMessages, AggregateException exception)
		{
			Success = success;
			FailedUrls = failedUrls ?? new Dictionary<CdnType, string[]>();
			FailMessages = failMessages ?? new string[0];
			Exception = exception;
		}

		public PurgeResponse(bool success, CdnType cdnType, IEnumerable<string> failedUrls, IEnumerable<string> failMessages, AggregateException exception)
		{
			Success = success;
			FailedUrls = failedUrls?.Any() ?? false ? new Dictionary<CdnType, string[]>
			{
				{ cdnType, failedUrls.ToArray() }
			} : new Dictionary<CdnType, string[]>();
			FailMessages = failMessages ?? new string[0];
			Exception = exception;
		}

		public bool Success { get; }

		public IDictionary<CdnType, string[]> FailedUrls { get; }
		public IEnumerable<string> FailMessages { get; }
		public AggregateException Exception { get; }
		
		public static PurgeResponse Aggregate(PurgeResponse[] responses)
		{
			var exceptions = responses
				.SelectMany(r => r.Exception?.InnerExceptions.AsEnumerable() ?? new Exception[0])
				.ToArray();

			return new PurgeResponse(
				success: responses.All(r => r.Success),
				failedUrls: responses.SelectMany(r=> r.FailedUrls)
					.GroupBy(r=> r.Key)
					.ToDictionary(g=> g.Key, g=> g.SelectMany(r=> r.Value).ToArray()),
				failMessages: responses.SelectMany(r => r.FailMessages),
				exception: exceptions.Any() ? new AggregateException(exceptions) : null
			);
		}
	}
}
