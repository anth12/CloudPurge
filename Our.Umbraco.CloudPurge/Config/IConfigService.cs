namespace Our.Umbraco.CloudPurge.Config
{
	public interface IConfigService
	{
		CloudPurgeConfig GetConfig();
		void WriteConfig(CloudPurgeConfig config);
	}
}