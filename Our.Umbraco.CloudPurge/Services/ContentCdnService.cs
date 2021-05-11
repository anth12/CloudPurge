using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Cdn;
using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Our.Umbraco.CloudPurge.Utilities;
using Umbraco.Cms.Core.Routing;
using Microsoft.Extensions.Options;

namespace Our.Umbraco.CloudPurge.Services
{
    public class ContentCdnService : IContentCdnService
	{
		private readonly IEnumerable<ICdnApi> _cdnApis;
		private readonly IOptionsMonitor<CloudPurgeConfig> _options;
		private readonly IContentTypeService _contentTypeService;
		private readonly IPublishedUrlProvider _urlProvider;

		public ContentCdnService(IEnumerable<ICdnApi> cdnApis, IOptionsMonitor<CloudPurgeConfig> options, IContentTypeService contentTypeService, IPublishedUrlProvider urlProvider)
		{
			_cdnApis = cdnApis;
			_options = options;
			_contentTypeService = contentTypeService;
			_urlProvider = urlProvider;
		}

		public async Task<PurgeResponse> PurgeAsync(IEnumerable<IPublishedContent> content)
		{
			var config = _options.CurrentValue;

			var publishedContent = content as IPublishedContent[] ?? content.ToArray();

			var contentTypeIds = publishedContent.Select(c => c.ContentType.Id).ToHashSet().ToArray();
			var contentTypes = _contentTypeService.GetAll(contentTypeIds).ToDictionary(c => c.Id, c => c);

			if (config.ContentFilter != null)
			{
				publishedContent = publishedContent
					.Where(c => config.ContentFilter.AllowedContent(contentTypes[c.ContentType.Id]))
					.ToArray();
			}

			var urls = from contentItem in publishedContent
						from culture in contentItem.Cultures
				select _urlProvider.GetUrl(contentItem, UrlMode.Absolute, culture.Key).ToString();

			if(!urls.Any())
				return new PurgeResponse(
					result: PurgeResult.NothingPurged,
					failedUrls: null,
					failMessages: null,
					exception: null);

			var request = new PurgeRequest(urls);

			return await PurgeAsync(request);
		}

		public async Task<PurgeResponse> PurgeAsync(PurgeRequest request)
		{
			var maxRequestSize = _cdnApis.Min(c => c.MaxBatchSize);

			var batches = request.Urls.Batch(maxRequestSize, c => {
				return new PurgeRequest(c);
				});

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
