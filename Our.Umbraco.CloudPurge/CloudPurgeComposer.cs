using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.Controllers;
using Our.Umbraco.CloudPurge.V4;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Dashboards;
using Umbraco.Core.Models.Sections;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: InternalsVisibleTo("Our.Umbraco.CloudPurge.Tests")]
namespace Our.Umbraco.CloudPurge
{
	public class CloudPurgeComposer : IComposer
	{
		public void Compose(Composition composition)
		{
			composition.Register<ICloudFlareConfigFactory, CloudFlareConfigFactory>(Lifetime.Singleton);
			composition.Register<ICloudFlareApi, CloudFlareApi>(Lifetime.Transient);
			composition.Register<CloudPurgeApiController>(Lifetime.Transient);
			
			composition.Register<HttpClient>(Lifetime.Singleton);
			
			composition.Components().Append<CloudPurgeComponent>();
			composition.Dashboards().Add<CloudPurgeDashboard>();

			ContentTreeController.MenuRendering += ContentTreeController_MenuRendering;
			//composition.HealthChecks().Add<CloudFlareHealthCheck>();
		}

		private static MenuItem CloudPurgeMenuItem = new MenuItem("cloudPurge", "Purge CloudFlare Cache")
		{
			Icon = "cloud",
			OpensDialog = true
		};

		private void ContentTreeController_MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
		{
			CloudPurgeMenuItem.LaunchDialogView("/App_Plugins/CloudPurge/backoffice/content/purge.html", "Purge CloudFlare");
			e.Menu.Items.Add(CloudPurgeMenuItem);
		}
	}


	[Weight(100)]
	public class CloudPurgeDashboard : IDashboard
	{
		public string Alias => "Our.Umbraco.CloudPurge";

		public string[] Sections => new[]
		{
			"settings"
		};

		public string View => "/App_Plugins/CloudPurge/dashboard.html";

		IAccessRule[] IDashboard.AccessRules { get; } = new IAccessRule[]
		{
			new AccessRule {Type = AccessRuleType.Grant, Value = "admin"}
		};

	}

	public class CloudPurgeComponent : IComponent
	{
		private ICloudFlareApi _cloudFlareService;

		public CloudPurgeComponent(ICloudFlareApi cloudFlareService)
		{
			_cloudFlareService = cloudFlareService;
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
			foreach (var content in e.PublishedEntities)
			{

			}
		}

		private void ContentService_Publishing(global::Umbraco.Core.Services.IContentService sender, global::Umbraco.Core.Events.ContentPublishingEventArgs e)
		{
			foreach (var content in e.PublishedEntities)
			{

			}
		}

		public void Terminate()
		{

		}
	}
}
