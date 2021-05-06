using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Trees;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Extensions;

namespace Our.Umbraco.CloudPurge.Events
{
    internal class MenuRenderingHandler : INotificationHandler<MenuRenderingNotification>
	{
		private readonly ILocalizedTextService _localizedTextService;

        public MenuRenderingHandler(ILocalizedTextService localizedTextService)
        {
			_localizedTextService = localizedTextService;

		}
				
		public void Handle(MenuRenderingNotification notification)
        {
			if (notification.TreeAlias == "content")
			{
				var cloudPurgeMenuItem = new MenuItem("cloudPurge", _localizedTextService.Localize("cloudpurge/action"))
				{
					Icon = "cloud",
					OpensDialog = true
				};

				cloudPurgeMenuItem.LaunchDialogView("/App_Plugins/CloudPurge/action.html", 
					_localizedTextService.Localize("cloudpurge/action"));

				notification.Menu.Items.Add(cloudPurgeMenuItem);
			}
		}
    }
}
