using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Web.HealthCheck;

namespace Our.Umbraco.CloudPurge.V4
{
	[HealthCheck("09C6BEB0-D68D-4DF6-8E0A-3E0972B2112C", "CloudPurge")]
	internal class CloudFlareHealthCheck : HealthCheck
	{
		private readonly ICloudFlareApi _cloudFlareApi;

		public CloudFlareHealthCheck(ICloudFlareApi cloudFlareApi)
		{
			_cloudFlareApi = cloudFlareApi;
		}

		public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
		{
						var result = Task.Run(() => _cloudFlareApi.ZoneDetailsAsync("")).Result;
			return new HealthCheckStatus("") { ResultType = StatusResultType.Info };
		}

		public override IEnumerable<HealthCheckStatus> GetStatus()
		{
			yield return new HealthCheckStatus("") { ResultType = StatusResultType.Info };
		}
	}
}
