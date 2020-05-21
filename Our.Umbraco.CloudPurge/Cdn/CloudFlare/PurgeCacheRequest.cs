using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Our.Umbraco.CloudPurge.Cdn.CloudFlare
{
	[ExcludeFromCodeCoverage]
	internal class PurgeCacheRequest
	{
		public PurgeCacheRequest(IEnumerable<string> files, bool purgeEverything)
		{
			Files = files;
			PurgeEverything = purgeEverything;
		}

		[JsonProperty("files")]
		public IEnumerable<string> Files { get; }

		[JsonProperty("purge_everything")]
		public bool PurgeEverything { get; }
	}
}
