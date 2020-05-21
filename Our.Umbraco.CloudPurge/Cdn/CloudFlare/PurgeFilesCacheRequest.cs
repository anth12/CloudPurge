using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Our.Umbraco.CloudPurge.Cdn.CloudFlare
{
	[ExcludeFromCodeCoverage]
	internal class PurgeFilesCacheRequest
	{
		public PurgeFilesCacheRequest(IEnumerable<string> files)
		{
			Files = files;
		}

		[JsonProperty("files")]
		public IEnumerable<string> Files { get; }
	}
}
