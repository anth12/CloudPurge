using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.Models;
using System.Threading.Tasks;
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Our.Umbraco.CloudPurge.Controllers
{
	[PluginController("CloudPurge")]
	public class CloudPurgeApiController : UmbracoAuthorizedApiController
	{
		private readonly ICloudFlareConfigFactory _configFactory;
		private readonly ICloudFlareApi _cloudFlareApi;

		public CloudPurgeApiController(ICloudFlareConfigFactory configFactory, ICloudFlareApi cloudFlareApi)
		{
			_configFactory = configFactory;
			_cloudFlareApi = cloudFlareApi;
		}

		[HttpGet]
		public async Task<CloudFlareConfig> Config()
		{
			var config = _configFactory.GetSettings();
			return config;
		}

		[HttpPost]
		public async Task<CloudFlareConfig> Config(CloudFlareConfig config)
		{
			// TODO persist
			return new CloudFlareConfig(config.EnablePublishHooks, config.EmailAddress + " (1)", config.Token, config.ZoneId);
		}

		[HttpGet]
		public async Task<bool> PurgeAll()
		{
			var request = new PurgeRequest(null, true);
			var result = await _cloudFlareApi.PurgeAsync(request);
			return true;
		}

		[HttpGet]
		public async Task<bool> Purge(int id, bool descendants = false)
		{
			//var request = new PurgeRequest(null, true);
			//var result = await _cloudFlareApi.PurgeAsync(request);
			return true;
		}
	}
}
