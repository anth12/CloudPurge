using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Dashboards;

namespace Our.Umbraco.CloudPurge
{
	[Weight(100)]
	public class CloudPurgeDashboard : IDashboard
	{
		public string Alias => "Our.Umbraco.CloudPurge";

		public string[] Sections => new[]
		{
			Constants.Applications.Settings
		};

		public string View => "/App_Plugins/CloudPurge/dashboard.html";

		IAccessRule[] IDashboard.AccessRules { get; } = {
			new AccessRule {Type = AccessRuleType.Grant, Value = "admin"}
		};

	}
}
