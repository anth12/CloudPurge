using System.Runtime.CompilerServices;
using Our.Umbraco.CloudPurge.Events;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Core.Services.Notifications;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Our.Umbraco.CloudPurge.Tests")]
namespace Our.Umbraco.CloudPurge
{
	public class CloudPurgeComposer : IComposer
	{
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Dashboards().Add<CloudPurgeDashboard>();

            builder.AddNotificationHandler<MenuRenderingNotification, MenuRenderingHandler>();
            builder.AddNotificationAsyncHandler<ContentTreeChangeNotification, ContentUpdateHandler>();
        }
	}
}
