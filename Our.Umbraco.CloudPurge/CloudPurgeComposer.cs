using System.Runtime.CompilerServices;
using Our.Umbraco.CloudPurge.Events;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Core.Services.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.CDN.CloudFlare;
using Our.Umbraco.CloudPurge.Services;
using Our.Umbraco.CloudPurge.Controllers;
using Our.Umbraco.CloudPurge.Cdn;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Our.Umbraco.CloudPurge.Tests")]
namespace Our.Umbraco.CloudPurge
{
	public class CloudPurgeComposer : IComposer
	{
        public void Compose(IUmbracoBuilder builder)
        {
			builder.Services.AddSingleton<IConfigService, ConfigFileService>();
			builder.Services.AddTransient<ICdnApi, CloudFlareV4Api>();
			builder.Services.AddTransient<IContentCdnService, ContentCdnService>();
			builder.Services.AddTransient<CloudPurgeApiController>();
			
			//composition.Register<HttpClient>(Lifetime.Singleton);
			
			builder.Dashboards().Add<CloudPurgeDashboard>();

			builder.AddNotificationHandler<MenuRenderingNotification, MenuRenderingHandler>();
			builder.AddNotificationAsyncHandler<ContentTreeChangeNotification, ContentUpdateHandler>();
		}
	}
}
