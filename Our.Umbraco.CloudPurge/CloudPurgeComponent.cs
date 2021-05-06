using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web;

namespace Our.Umbraco.CloudPurge
{
	internal class CloudPurgeComponent : IComponent
	{
		private readonly ILogger<CloudPurgeComponent> _logger;
		private readonly IConfigService _configService;
		private readonly IContentCdnService _contentCdnService;
		private readonly IUmbracoContextFactory _umbracoContextFactory;

		public CloudPurgeComponent(ILogger<CloudPurgeComponent> logger, IConfigService configService, IContentCdnService contentCdnService, IUmbracoContextFactory umbracoContextFactory)
		{
			_logger = logger;
			_configService = configService;
			_contentCdnService = contentCdnService;
			_umbracoContextFactory = umbracoContextFactory;
		}

		public void Initialize()
		{
			ContentService.Published += ContentService_Published;
			ContentService.Unpublishing += ContentService_Unpublishing;

			ContentService.Trashed += ContentService_Moved;
			ContentService.Moved += ContentService_Moved;
		}

		private void ContentService_Published(IContentService sender, ContentPublishedEventArgs e)
		{
			if (!PublishHookEnabled())
				return;

			var contentIds = e.PublishedEntities.Select(c => c.Id);

			PurgeCache(e.Messages, contentIds);
		}

		private void ContentService_Unpublishing(IContentService sender, PublishEventArgs<global::Umbraco.Core.Models.IContent> e)
		{
			if (!PublishHookEnabled())
				return;

			var contentIds = e.PublishedEntities.Select(c => c.Id);

			PurgeCache(e.Messages, contentIds);
		}

		private void ContentService_Moved(IContentService sender, MoveEventArgs<global::Umbraco.Core.Models.IContent> e)
		{
			if (!PublishHookEnabled())
				return;

			var contentIds = e.MoveInfoCollection.Select(c => c.Entity.Id);

			PurgeCache(e.Messages, contentIds);
		}

		private bool PublishHookEnabled()
			=> _configService.GetConfig().EnablePublishHooks;

		private void PurgeCache(EventMessages messages, IEnumerable<int> contentIds)
		{
			using (var context = _umbracoContextFactory.EnsureUmbracoContext())
			{
				var content = contentIds.Select(context.UmbracoContext.Content.GetById);

				try
				{
					var result = Task.Run(() => _contentCdnService.PurgeAsync(content)).GetAwaiter().GetResult();
					
					if (result.Result == Models.PurgeResult.Success)
					{
						messages.Add(new EventMessage("CloudPurge",
							"Cleared CDN cache", EventMessageType.Success));
					}
					else if (result.Result == Models.PurgeResult.Fail)
					{
						_logger.Error<CloudPurgeComposer>("Failed to purge cache for {FailedUrlCount} urls ((failedUrls}). With messages {failMessages}",
							result.FailedUrls.Count(),
							result.FailedUrls,
							result.FailMessages);

						messages.Add(new EventMessage("CloudPurge",
							"Something went wrong clearing CDN cache", EventMessageType.Warning));

						foreach (var failMessage in result.FailMessages.Take(5))
						{
							messages.Add(new EventMessage("CloudPurge", failMessage, EventMessageType.Error));
						}
					}
				}
				catch (TimeoutException)
				{
					messages.Add(new EventMessage("CloudPurge",
						"Looks like it's taking a little while to clear the CDN cache...", EventMessageType.Warning));
				}
				catch (Exception ex)
				{
					_logger.Error<CloudPurgeComposer>("Failed to purge cache", ex);

					messages.Add(new EventMessage("CloudPurge",
						"Something went wrong clearing CDN cache", EventMessageType.Error));
				}
			}
		}
		
		public void Terminate()
		{
			_contentCdnService.Dispose();
		}
	}
}
