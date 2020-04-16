using Our.Umbraco.CloudPurge.Services;
using Umbraco.Core.Composing;
using Umbraco.Core.Events;
using Umbraco.Core.Services.Implement;

namespace Our.Umbraco.CloudPurge
{
	internal class CloudPurgeComponent : IComponent
	{
		private readonly IContentCdnService _contentCdnService;

		public CloudPurgeComponent(IContentCdnService contentCdnService)
		{
			_contentCdnService = contentCdnService;
		}

		public void Initialize()
		{
			ContentService.Publishing += ContentService_Publishing;
			ContentService.Unpublishing += ContentService_Unpublishing;
			ContentService.Trashed += ContentService_Trashed;
		}

		private void ContentService_Trashed(global::Umbraco.Core.Services.IContentService sender, global::Umbraco.Core.Events.MoveEventArgs<global::Umbraco.Core.Models.IContent> e)
		{
			// Required? Surely it's unpublished first

		}

		private void ContentService_Unpublishing(global::Umbraco.Core.Services.IContentService sender, global::Umbraco.Core.Events.PublishEventArgs<global::Umbraco.Core.Models.IContent> e)
		{
			_contentCdnService.PurgeAsync(e.PublishedEntities);
			e.Messages.Add(new EventMessage("test", "some message", EventMessageType.Info));
		}

		private void ContentService_Publishing(global::Umbraco.Core.Services.IContentService sender, global::Umbraco.Core.Events.ContentPublishingEventArgs e)
		{
			_contentCdnService.PurgeAsync(e.PublishedEntities);
			e.Messages.Add(new EventMessage("test", "some message", EventMessageType.Info));
		}

		public void Terminate()
		{
			_contentCdnService.Dispose();
		}
	}
}
