using System;
using System.Collections.Generic;
using System.Linq;

namespace Our.Umbraco.CloudPurge.Models
{
	public class PurgeResponse
	{
		public PurgeResponse(bool success, IEnumerable<string> failedUrls, IEnumerable<string> failMessages, AggregateException exception)
		{
			Success = success;
			FailedUrls = failedUrls ?? new string[0];
			FailMessages = failMessages ?? new string[0];
			Exception = exception;
		}

		public bool Success { get; }

		public IEnumerable<string> FailedUrls { get; }
		public IEnumerable<string> FailMessages { get; }
		public AggregateException Exception { get; }

		public static PurgeResponse Aggregate(PurgeResponse[] responses)
		{
			var exceptions = responses
				.SelectMany(r => r.Exception?.InnerExceptions.AsEnumerable() ?? new Exception[0])
				.ToArray();

			return new PurgeResponse(
				success: responses.All(r => r.Success),
				failedUrls: responses.SelectMany(r => r.FailedUrls),
				failMessages: responses.SelectMany(r => r.FailMessages),
				exception: exceptions.Any() ? new AggregateException(exceptions) : null
			);
		}
	}
}
