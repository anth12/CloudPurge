using System;
using System.Collections.Generic;

namespace Our.Umbraco.CloudPurge.Models
{
	public class PurgeResponse
	{
		public PurgeResponse(bool success, IEnumerable<string> failedUrls, IEnumerable<string> failMessages, AggregateException exception)
		{
			Success = success;
			FailedUrls = failedUrls;
			FailMessages = failMessages;
			Exception = exception;
		}

		public bool Success { get; }

		public IEnumerable<string> FailedUrls { get; }
		public IEnumerable<string> FailMessages { get; }
		public AggregateException Exception { get; }
	}
}
