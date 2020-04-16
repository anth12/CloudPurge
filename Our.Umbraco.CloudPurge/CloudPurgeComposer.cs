using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.Controllers;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Our.Umbraco.CloudPurge.Cdn;
using Our.Umbraco.CloudPurge.CDN.CloudFlare;
using Our.Umbraco.CloudPurge.Services;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web;
using Umbraco.Web.Trees;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Our.Umbraco.CloudPurge.Tests")]
namespace Our.Umbraco.CloudPurge
{
	public class CloudPurgeComposer : IComposer
	{
		public void Compose(Composition composition)
		{
			composition.Register<IConfigService, ConfigFileService>(Lifetime.Singleton);
			composition.Register<ICdnApi, CloudFlareV4Api>(Lifetime.Transient);
			composition.Register<IContentCdnService, ContentCdnService>(Lifetime.Transient);
			composition.Register<CloudPurgeApiController>(Lifetime.Transient);
			
			composition.Register<HttpClient>(Lifetime.Singleton);
			
			composition.Components().Append<CloudPurgeComponent>();
			composition.Dashboards().Add<CloudPurgeDashboard>();

			// ReSharper disable once AccessToStaticMemberViaDerivedType
			ContentTreeController.MenuRendering += CloudPurgeAction.ContentTreeController_MenuRendering;
		}

	}
}
