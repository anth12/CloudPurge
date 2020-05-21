using System.Runtime.Serialization;

namespace Our.Umbraco.CloudPurge.Config
{
	[DataContract]
	public class CloudPurgeConfig
	{
		public CloudPurgeConfig()
		{
			CloudFlare = new CloudFlareConfig(false, "", "", "");
		}

		public CloudPurgeConfig(bool enablePublishHooks, ContentFilterConfig contentFilter, CloudFlareConfig cloudFlare)
		{
			EnablePublishHooks = enablePublishHooks;
			ContentFilter = contentFilter;
			CloudFlare = cloudFlare;
		}

		[DataMember]
		public bool EnablePublishHooks { get; set; }

		[DataMember]
		public ContentFilterConfig ContentFilter { get; set; }

		[DataMember]
		public CloudFlareConfig CloudFlare { get; set; }
		
	}
}
