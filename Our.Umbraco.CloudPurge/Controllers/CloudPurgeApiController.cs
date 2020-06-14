using System.Collections.Generic;
using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.Models;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Our.Umbraco.CloudPurge.Services;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Our.Umbraco.CloudPurge.Controllers
{
	[PluginController("CloudPurge")]
	public class CloudPurgeApiController : UmbracoAuthorizedApiController
	{
		private readonly IConfigService _configService;
		private readonly IContentCdnService _cdnService;

		public CloudPurgeApiController(IConfigService configService, IContentCdnService cdnService)
		{
			_configService = configService;
			_cdnService = cdnService;
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
			var request = new PurgeAllRequest();
			var result = await _cdnService.PurgeAllAsync(request);
			return result;
		}

		[HttpGet]
		public async Task<PurgeResponse> Purge(int id, bool descendants = false)
		{
			var content = UmbracoContext.Content.GetById(id);

			if(content == null)
				throw new HttpException(404, $"Content {id} not found");

			if (descendants)
			{
				var descendents = GetDescendents(content);
				return await _cdnService.PurgeAsync(descendents);
			}

			return await _cdnService.PurgeAsync(new[] {content});
		}

		private IEnumerable<IPublishedContent> GetDescendents(IPublishedContent parent)
		{
			yield return parent;

			if(parent.Children == null)
				yield break;

			foreach (var child in parent.Children)
			foreach (var grandChild in GetDescendents(child))
			{
				yield return grandChild;
			}
		}
	}
}
