using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Trees;

namespace Our.Umbraco.CloudPurge
{
	internal class CloudPurgeAction
	{
		private static ILocalizedTextService LocalizedTextService => Current.Services.TextService;

		private static MenuItem CloudPurgeMenuItem = new MenuItem("cloudPurge", LocalizedTextService.Localize("cloudpurge/action"))
		{
			Icon = "cloud",
			OpensDialog = true
		};

		public static void ContentTreeController_MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
		{
			CloudPurgeMenuItem.LaunchDialogView("/App_Plugins/CloudPurge/action.html", LocalizedTextService.Localize("cloudpurge/action"));
			e.Menu.Items.Add(CloudPurgeMenuItem);
		}
	}
}
