using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Our.Umbraco.CloudPurge.V4
{
	public class Account
	{

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }
	}

	public class Plan
	{

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("price")]
		public int Price { get; set; }

		[JsonProperty("currency")]
		public string Currency { get; set; }

		[JsonProperty("frequency")]
		public string Frequency { get; set; }

		[JsonProperty("legacy_id")]
		public string LegacyId { get; set; }

		[JsonProperty("is_subscribed")]
		public bool IsSubscribed { get; set; }

		[JsonProperty("can_subscribe")]
		public bool CanSubscribe { get; set; }
	}

	public class PlanPending
	{

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("price")]
		public int Price { get; set; }

		[JsonProperty("currency")]
		public string Currency { get; set; }

		[JsonProperty("frequency")]
		public string Frequency { get; set; }

		[JsonProperty("legacy_id")]
		public string LegacyId { get; set; }

		[JsonProperty("is_subscribed")]
		public bool IsSubscribed { get; set; }

		[JsonProperty("can_subscribe")]
		public bool CanSubscribe { get; set; }
	}

	public class ZoneDetailsResult
	{

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("development_mode")]
		public int DevelopmentMode { get; set; }

		[JsonProperty("original_name_servers")]
		public IList<string> OriginalNameServers { get; set; }

		[JsonProperty("original_registrar")]
		public string OriginalRegistrar { get; set; }

		[JsonProperty("original_dnshost")]
		public string OriginalDnshost { get; set; }

		[JsonProperty("created_on")]
		public DateTime CreatedOn { get; set; }

		[JsonProperty("modified_on")]
		public DateTime ModifiedOn { get; set; }

		[JsonProperty("activated_on")]
		public DateTime ActivatedOn { get; set; }

		[JsonProperty("account")]
		public Account Account { get; set; }

		[JsonProperty("permissions")]
		public IList<string> Permissions { get; set; }

		[JsonProperty("plan")]
		public Plan Plan { get; set; }

		[JsonProperty("plan_pending")]
		public PlanPending PlanPending { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("paused")]
		public bool Paused { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("name_servers")]
		public IList<string> NameServers { get; set; }
	}

}
