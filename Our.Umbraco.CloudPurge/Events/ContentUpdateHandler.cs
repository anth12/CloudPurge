using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Services.Notifications;
using Microsoft.Extensions.Logging;
using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.Services;
using Umbraco.Cms.Core.Web;

namespace Our.Umbraco.CloudPurge.Events
{
    internal class ContentUpdateHandler : INotificationAsyncHandler<ContentTreeChangeNotification>
	{
		private readonly ILogger<MenuRenderingHandler> _logger;
		private readonly IConfigService _configService;
		private readonly IContentCdnService _contentCdnService;
		private readonly IUmbracoContextFactory _umbracoContextFactory;

		public ContentUpdateHandler(ILogger<MenuRenderingHandler> logger, IConfigService configService, IContentCdnService contentCdnService, IUmbracoContextFactory umbracoContextFactory)
		{
			_logger = logger;
			_configService = configService;
			_contentCdnService = contentCdnService;
			_umbracoContextFactory = umbracoContextFactory;
		}

		private bool PublishHookEnabled()
			=> _configService.GetConfig().EnablePublishHooks;

		public Task HandleAsync(ContentSavingNotification notification, CancellationToken cancellationToken)
		{
			if (!PublishHookEnabled())
				return Task.CompletedTask;

			var contentIds = notification.SavedEntities.Select(c => c.Id);

			return PurgeCacheAsync(notification.Messages, contentIds);
		}

		public Task HandleAsync(ContentTreeChangeNotification notification, CancellationToken cancellationToken)
		{
			if (!PublishHookEnabled())
				return Task.CompletedTask;

			var contentIds = notification.Changes.Select(c => c.Item.Id);

			return PurgeCacheAsync(notification.Messages, contentIds);
		}

		private async Task PurgeCacheAsync(EventMessages messages, IEnumerable<int> contentIds)
		{
			using (var context = _umbracoContextFactory.EnsureUmbracoContext())
			{
				var content = contentIds.Select(context.UmbracoContext.Content.GetById);

				try
				{
					var result = await _contentCdnService.PurgeAsync(content);

					if (result.Result == Models.PurgeResult.Success)
					{
						messages.Add(new EventMessage("CloudPurge",
							"Cleared CDN cache", EventMessageType.Success));
					}
					else if (result.Result == Models.PurgeResult.Fail)
					{
						_logger.LogError("Failed to purge cache for {FailedUrlCount} urls ((failedUrls}). With messages {failMessages}",
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
					_logger.LogError(ex, "Failed to purge cache");

					messages.Add(new EventMessage("CloudPurge",
						"Something went wrong clearing CDN cache", EventMessageType.Error));
				}
			}
		}

    }
}
