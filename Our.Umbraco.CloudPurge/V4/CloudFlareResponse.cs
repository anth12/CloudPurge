using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Our.Umbraco.CloudPurge.V4
{
	[ExcludeFromCodeCoverage]
	internal class CloudFlareResponse<TResult>
	{
		[JsonProperty("result")]
		public TResult Result { get; set; }
		[JsonProperty("success")]
		public bool Success { get; set; }
		[JsonProperty("errors")]
		public List<CloudFlareError> Errors { get; set; }
		[JsonProperty("messages")]
		public List<string> Messages { get; set; }

	}
}
