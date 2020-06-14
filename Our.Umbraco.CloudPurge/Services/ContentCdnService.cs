using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Cdn;
using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web;
using Our.Umbraco.CloudPurge.Utilities;

namespace Our.Umbraco.CloudPurge.Services
{
	public class ContentCdnService : IContentCdnService
	{
		private readonly IEnumerable<ICdnApi> _cdnApis;
		private readonly IConfigService _configService;
		private readonly IContentTypeService _contentTypeService;
		private readonly IUmbracoContextFactory _umbracoContextFactory;

		public ContentCdnService(IEnumerable<ICdnApi> cdnApis, IConfigService configService, IContentTypeService contentTypeService, IUmbracoContextFactory umbracoContextFactory)
		{
			_cdnApis = cdnApis;
			_configService = configService;
			_contentTypeService = contentTypeService;
			_umbracoContextFactory = umbracoContextFactory;
		}

		public Task<PurgeResponse> PurgeAsync(IEnumerable<IPublishedContent> content)
		{
			var config = _configService.GetConfig();

			var publishedContent = content as IPublishedContent[] ?? content.ToArray();

			var contentTypeIds = publishedContent.Select(c => c.ContentType.Id).ToHashSet().ToArray();
			var contentTypes = _contentTypeService.GetAll(contentTypeIds).ToDictionary(c => c.Id, c => c);

			var filteredContent =
				publishedContent.Where(c => config.ContentFilter.AllowedContent(contentTypes[c.ContentType.Id]));

			using (var context = _umbracoContextFactory.EnsureUmbracoContext())
			{
				var urls = from contentItem in filteredContent
					from culture in contentItem.Cultures
					select context.UmbracoContext.UrlProvider.GetUrl(contentItem, UrlMode.Absolute, culture.Key);

				var request = new PurgeRequest(urls);

				return PurgeAsync(request);
			}
		}

		public async Task<PurgeResponse> PurgeAsync(PurgeRequest request)
		{
			var maxRequestSize = _cdnApis.Min(c => c.MaxBatchSize);

			var batches = request.Urls.Batch(maxRequestSize, c => new PurgeRequest(c));

			var purgeTasks = _cdnApis.Where(c => c.IsEnabled())
				.SelectMany(cdn => batches.Select(cdn.PurgeByUrlAsync));
			
			var results = await Task.WhenAll(purgeTasks);

			return PurgeResponse.Aggregate(results);
		}

		public async Task<PurgeResponse> PurgeAllAsync(PurgeAllRequest request)
		{
			var purgeTasks = _cdnApis.Where(c => c.IsEnabled())
				.Select(cdn => cdn.PurgeAllAsync(request));
			
			var results = await Task.WhenAll(purgeTasks);

			return PurgeResponse.Aggregate(results);
		}

		public void Dispose()
		{
			if (_cdnApis == null) 
				return;

			foreach (var cdnApi in _cdnApis)
			{
				cdnApi?.Dispose();
			}
		}
	}
}
