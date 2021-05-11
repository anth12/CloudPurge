using System.Collections.Generic;
using Our.Umbraco.CloudPurge.Models;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Services;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Attributes;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Web;

namespace Our.Umbraco.CloudPurge.Controllers
{
	[PluginController("CloudPurge")]
	public class CloudPurgeApiController : UmbracoAuthorizedApiController
	{
		private readonly IContentCdnService _cdnService;
		private readonly IUmbracoContextAccessor _umbracoContextAccessor;

		public CloudPurgeApiController(IContentCdnService cdnService, IUmbracoContextAccessor umbracoContextAccessor)
		{
			_cdnService = cdnService;
			_umbracoContextAccessor = umbracoContextAccessor;
		}

		[HttpGet]
		public async Task<PurgeResponse> PurgeAll()
		{
			var request = new PurgeAllRequest();
			var result = await _cdnService.PurgeAllAsync(request);
			return result;
		}

		public async Task<IActionResult> Purge(int id, bool descendants = false)
		{
			
			var content = _umbracoContextAccessor.UmbracoContext.Content.GetById(id);

			if(content == null)
				return NotFound($"Content {id} not found");

			PurgeResponse result;
			if (descendants)
			{
				var descendents = GetDescendents(content);
				result = await _cdnService.PurgeAsync(descendents);
				return Ok(result);
			}

			result = await _cdnService.PurgeAsync(new[] {content});
			return Ok(result);
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
