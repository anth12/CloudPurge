namespace Our.Umbraco.CloudPurge.Config
{
	public interface ICloudFlareConfigFactory
	{
		CloudFlareConfig GetSettings();
	}
}