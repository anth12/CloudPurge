using System.Runtime.Serialization;

namespace Our.Umbraco.CloudPurge.Config
{
	[DataContract]
	public class CloudFlareConfig : ICdnConfig
	{
		public CloudFlareConfig() {}

		public CloudFlareConfig(bool enabled, string emailAddress, string token, string zoneId)
		{
			Enabled = enabled;
			EmailAddress = emailAddress;
			Token = token;
			ZoneId = zoneId;
		}

		[DataMember]
		public bool Enabled { get; set; }

		[DataMember]
		public string EmailAddress { get; set; }

		[DataMember]
		public string Token { get; set; }

		[DataMember]
		public string ZoneId { get; set; }

		public bool IsValid()
			=> !string.IsNullOrEmpty(EmailAddress) &&
			!string.IsNullOrEmpty(Token) &&
			!string.IsNullOrEmpty(ZoneId);
	}
}
