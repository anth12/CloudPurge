
namespace Our.Umbraco.CloudPurge.Config
{
	internal interface ICdnConfig
	{
		bool Enabled { get; }

		bool IsValid();
	}
}
