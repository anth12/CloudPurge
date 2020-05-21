using System.Linq;
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
			ContentService.Trashed += ContentService_Trashed;
		}

		private void ContentService_Published(IContentService sender, ContentPublishedEventArgs e)
		{
			var contentIds = e.PublishedEntities.Select(c => c.Id);

			using (var context = _umbracoContextFactory.EnsureUmbracoContext())
			{
				var content = contentIds.Select(context.UmbracoContext.Content.GetById);
				_contentCdnService.PurgeAsync(content);
			}
			
			e.Messages.Add(new EventMessage("test", "some message", EventMessageType.Info));

		}

		private void ContentService_Unpublishing(IContentService sender, PublishEventArgs<global::Umbraco.Core.Models.IContent> e)
		{
			var contentIds = e.PublishedEntities.Select(c => c.Id);

			using (var context = _umbracoContextFactory.EnsureUmbracoContext())
			{
				var content = contentIds.Select(context.UmbracoContext.Content.GetById);
				_contentCdnService.PurgeAsync(content);
			}

			e.Messages.Add(new EventMessage("test", "some message", EventMessageType.Info));
		}

		private void ContentService_Trashed(IContentService sender, MoveEventArgs<global::Umbraco.Core.Models.IContent> e)
		{
			// Required? Surely it's unpublished first

		}


		public void Terminate()
		{
			_contentCdnService.Dispose();
		}
	}
}
