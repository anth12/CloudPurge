using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.Controllers;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Our.Umbraco.CloudPurge.Cdn;
using Our.Umbraco.CloudPurge.CDN.CloudFlare;
using Our.Umbraco.CloudPurge.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.BackOffice.Trees;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Our.Umbraco.CloudPurge.Tests")]
namespace Our.Umbraco.CloudPurge
{
	public class CloudPurgeComposer : IComposer
	{
		public void Compose(Composition composition)
		{
		}

        public void Compose(IUmbracoBuilder builder)
        {
			//builder.Register<IConfigService, ConfigFileService>(Lifetime.Singleton);
			//composition.Register<ICdnApi, CloudFlareV4Api>(Lifetime.Transient);
			//composition.Register<IContentCdnService, ContentCdnService>(Lifetime.Transient);
			//composition.Register<CloudPurgeApiController>(Lifetime.Transient);
			
			//composition.Register<HttpClient>(Lifetime.Singleton);
			
			builder.Components().Append<CloudPurgeComponent>();
			builder.Dashboards().Add<CloudPurgeDashboard>();

			builder.AddNotificationHandler<MenuRenderingNotification, MenuRenderingHandler>();


		}
	}
}
