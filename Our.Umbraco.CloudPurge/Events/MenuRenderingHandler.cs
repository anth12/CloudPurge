using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Web.BackOffice.Trees;

namespace Our.Umbraco.CloudPurge.Events
{
    internal class MenuRenderingHandler : INotificationHandler<MenuRenderingNotification>
	{

		//private static ILocalizedTextService LocalizedTextService => Current.Services.TextService;

		private static readonly MenuItem CloudPurgeMenuItem = new MenuItem("cloudPurge", "cloudpurge/action") // TODO
		{
			Icon = "cloud",
			OpensDialog = true
		};

		public void Handle(MenuRenderingNotification notification)
        {
			if (notification.TreeAlias == "content")
			{
				CloudPurgeMenuItem.LaunchDialogView("/App_Plugins/CloudPurge/action.html",
					"cloudpurge/action");

				notification.Menu.Items.Add(CloudPurgeMenuItem);
			}
		}
    }
}
