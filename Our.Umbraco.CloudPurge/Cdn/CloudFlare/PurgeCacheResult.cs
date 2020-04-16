using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Our.Umbraco.CloudPurge.V4
{
	[ExcludeFromCodeCoverage]
	internal class PurgeCacheResult
	{
		[JsonProperty("id")]
		public string Id { get; set; }
		[JsonProperty("type")]
		public string Type { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("content")]
		public string Content { get; set; }
		[JsonProperty("proxiable")]
		public bool Proxiable { get; set; }
		[JsonProperty("proxied")]
		public bool Proxied { get; set; }
		[JsonProperty("ttl")]
		public int Ttl { get; set; }
		[JsonProperty("priority")]
		public int Priority { get; set; }
		[JsonProperty("locked")]
		public bool Locked { get; set; }
		[JsonProperty("zone id")]
		public string ZoneId { get; set; }
		[JsonProperty("zone name")]
		public string ZoneName { get; set; }
		[JsonProperty("modified on")]
		public DateTime ModifiedOn { get; set; }
		[JsonProperty("created_on")]
		public DateTime CreatedOn { get; set; }
	}

	[ExcludeFromCodeCoverage]
	internal class CloudFlareError
	{
		[JsonProperty("code")]
		public int Code { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }
	}
}
