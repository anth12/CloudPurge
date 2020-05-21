using System.Collections.Generic;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Cdn;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.CloudPurge.HealthCheck
{
	[HealthCheck("09C6BEB0-D68D-4DF6-8E0A-3E0972B2112C", "CloudPurge")]
	internal class CloudFlareHealthCheck : global::Umbraco.Web.HealthCheck.HealthCheck
	{
		private readonly IEnumerable<ICdnApi> _cdnApis;

		public CloudFlareHealthCheck(IEnumerable<ICdnApi> cdnApis)
		{
			_cdnApis = cdnApis;
		}

		public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
		{
			//var result = Task.Run(() => _cdnApi.HealthCheckAsync()).Result;
			return new HealthCheckStatus("Test execute") { ResultType = StatusResultType.Info };
		}

		public override IEnumerable<HealthCheckStatus> GetStatus()
		{
			foreach (var cdnApi in _cdnApis)
			{
				if (cdnApi.IsEnabled())
				{
					var result = Task.Run(() => cdnApi.HealthCheckAsync()).Result;

					yield return new HealthCheckStatus("CloudPurge")
					{
						ResultType = result ? StatusResultType.Success : StatusResultType.Warning
					};
				}

			}
		}
	}
}
