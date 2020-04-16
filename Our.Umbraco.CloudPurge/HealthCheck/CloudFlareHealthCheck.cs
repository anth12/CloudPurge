using System.Collections.Generic;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Cdn;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.CloudPurge.CDN.CloudFlare
{
	[HealthCheck("09C6BEB0-D68D-4DF6-8E0A-3E0972B2112C", "CloudPurge")]
	internal class CloudFlareHealthCheck : HealthCheck
	{
		private readonly ICdnApi _cdnApi;

		public CloudFlareHealthCheck(ICdnApi cdnApi)
		{
			_cdnApi = cdnApi;
		}

		public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
		{
			var result = Task.Run(() => _cdnApi.HealthCheckAsync()).Result;
			return new HealthCheckStatus("Test execute") { ResultType = StatusResultType.Info };
		}

		public override IEnumerable<HealthCheckStatus> GetStatus()
		{
			var result = Task.Run(() => _cdnApi.HealthCheckAsync()).Result;
			yield return new HealthCheckStatus("Cloud")
			{
				ResultType = StatusResultType.Info
			};
		}
	}
}
