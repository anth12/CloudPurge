using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Our.Umbraco.CloudPurge.Cdn.CloudFlare;

namespace Our.Umbraco.CloudPurge.CDN.CloudFlare
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
