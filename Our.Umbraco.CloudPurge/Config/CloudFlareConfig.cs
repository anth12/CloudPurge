
namespace Our.Umbraco.CloudPurge
{
	public class CloudFlareConfig
	{
		public CloudFlareConfig(bool enablePublishHooks, string emailAddress, string token, string zoneId)
		{
			EnablePublishHooks = enablePublishHooks;
			EmailAddress = emailAddress;
			Token = token;
			ZoneId = zoneId;
		}

		public bool EnablePublishHooks { get; }
		public string EmailAddress { get; }
		public string Token { get; }
		public string ZoneId { get; }
	}
}
