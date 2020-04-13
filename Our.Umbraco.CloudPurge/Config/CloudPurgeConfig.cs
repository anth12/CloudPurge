using System.Runtime.Serialization;

namespace Our.Umbraco.CloudPurge
{
	[DataContract]
	public class CloudPurgeConfig
	{
		public CloudPurgeConfig() {}

		public CloudPurgeConfig(bool enablePublishHooks, string emailAddress, string token, string zoneId)
		{
			EnablePublishHooks = enablePublishHooks;
			EmailAddress = emailAddress;
			Token = token;
			ZoneId = zoneId;
		}

		[DataMember]
		public bool EnablePublishHooks { get; set; }

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
