using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Our.Umbraco.CloudPurge.Cdn.CloudFlare
{
	[ExcludeFromCodeCoverage]
	internal class PurgeAllCacheRequest
	{
		public PurgeAllCacheRequest(bool purgeEverything)
		{
			PurgeEverything = purgeEverything;
		}
		
		[JsonProperty("purge_everything")]
		public bool PurgeEverything { get; }
	}
}
