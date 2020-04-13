
namespace Our.Umbraco.CloudPurge.Config
{
	internal class CloudFlareConfigFactory : ICloudFlareConfigFactory
	{
		private CloudFlareConfig _settings;

		public CloudFlareConfig GetSettings()
		{
			if (_settings != null)
				return _settings;

			return _settings = new CloudFlareConfig(true, "test", "sfsdf", "234");;
		}
	}
}
