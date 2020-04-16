using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.Models;
using System.Threading.Tasks;
using System.Web.Http;
using Our.Umbraco.CloudPurge.Cdn;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Our.Umbraco.CloudPurge.Controllers
{
	[PluginController("CloudPurge")]
	public class CloudPurgeApiController : UmbracoAuthorizedApiController
	{
		private readonly IConfigService _configService;
		private readonly ICdnApi _cdnApi;

		public CloudPurgeApiController(IConfigService configService, ICdnApi cdnApi)
		{
			_configService = configService;
			_cdnApi = cdnApi;
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
		public async Task<PurgeResponse> PurgeAll()
		{
			var request = new PurgeRequest(null, true);
			var result = await _cdnApi.PurgeAsync(request);
			return result;
		}

		[HttpGet]
		public async Task<bool> Purge(int id, bool descendants = false)
		{
			//var request = new PurgeRequest(null, true);
			//var result = await _cdnApi.PurgeAsync(request);
			return true;
		}
	}
}
