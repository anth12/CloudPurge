using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Services;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web;

namespace Our.Umbraco.CloudPurge
{
	internal class CloudPurgeComponent : IComponent
	{
		private readonly IContentCdnService _contentCdnService;
		private readonly IUmbracoContextFactory _umbracoContextFactory;

		public CloudPurgeComponent(IContentCdnService contentCdnService, IUmbracoContextFactory umbracoContextFactory)
		{
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
			var contentIds = e.PublishedEntities.Select(c => c.Id);

			PurgeCache(e.Messages, contentIds);
		}

		private void ContentService_Unpublishing(IContentService sender, PublishEventArgs<global::Umbraco.Core.Models.IContent> e)
		{
			var contentIds = e.PublishedEntities.Select(c => c.Id);

			PurgeCache(e.Messages, contentIds);
		}

		private void ContentService_Moved(IContentService sender, MoveEventArgs<global::Umbraco.Core.Models.IContent> e)
		{
			var contentIds = e.MoveInfoCollection.Select(c => c.Entity.Id);

			PurgeCache(e.Messages, contentIds);

		}

		private void PurgeCache(EventMessages messages, IEnumerable<int> contentIds)
		{
			using (var context = _umbracoContextFactory.EnsureUmbracoContext())
			{
				var content = contentIds.Select(context.UmbracoContext.Content.GetById);

				try
				{
					var result = Task.Run(() => _contentCdnService.PurgeAsync(content)).GetAwaiter().GetResult();
					
					if (result.Success)
					{
						messages.Add(new EventMessage("CloudPurge",
							"Cleared CDN cache", EventMessageType.Success));
					}
					else
					{
						messages.Add(new EventMessage("CloudPurge",
							$"Something went wrong clearing CDN cache", EventMessageType.Warning));

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
