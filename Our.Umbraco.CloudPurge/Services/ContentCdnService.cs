using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Cdn;
using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.Models;
using Serilog;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Our.Umbraco.CloudPurge.Services
{
	public class ContentCdnService : IContentCdnService
	{
		private readonly ILogger _logger;
		private readonly IEnumerable<ICdnApi> _cdnApis;
		private readonly IConfigService _configService;
		private readonly IContentTypeService _contentTypeService;

		public ContentCdnService(IEnumerable<ICdnApi> cdnApis, IConfigService configService, IContentTypeService contentTypeService)
		{
			_logger = Log.ForContext<ContentCdnService>();

			_cdnApis = cdnApis;
			_configService = configService;
			_contentTypeService = contentTypeService;
		}

		public Task<PurgeResponse> PurgeAsync(IEnumerable<IPublishedContent> content)
		{
			var config = _configService.GetConfig();

			var publishedContent = content as IPublishedContent[] ?? content.ToArray();

			var contentTypeIds = publishedContent.Select(c => c.ContentType.Id).ToHashSet().ToArray();
			var contentTypes = _contentTypeService.GetAll(contentTypeIds).ToDictionary(c=> c.Id, c=> c);

			var filteredContent = publishedContent.Where(c => config.ContentFilter.FilterContent(contentTypes[c.ContentType.Id]));

			var urls = from contentItem in filteredContent
					   from culture in contentItem.Cultures 
				select contentItem.Url(culture.Key, UrlMode.Absolute);
			
			var request = new PurgeRequest(urls, false);

			return PurgeAsync(request);
		}

		public async Task<PurgeResponse> PurgeAsync(PurgeRequest request)
		{
			var purgeTasks = _cdnApis.Where(c => c.IsEnabled()).Select(cdn => cdn.PurgeAsync(request));

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
