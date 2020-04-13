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
		private readonly IConfigService _configService;
		private readonly ICloudFlareApi _cloudFlareApi;

		public CloudPurgeApiController(IConfigService configService, ICloudFlareApi cloudFlareApi)
		{
			_configService = configService;
			_cloudFlareApi = cloudFlareApi;
		}

		[HttpGet]
		public CloudPurgeConfig Config()
		{
			var config = _configService.GetConfig();
			return config;
		}

		[HttpPost]
		public CloudPurgeConfig Config(CloudPurgeConfig config)
		{
			_configService.WriteConfig(config);
			return config;
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
