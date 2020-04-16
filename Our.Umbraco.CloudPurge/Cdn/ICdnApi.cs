using System;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Models;

namespace Our.Umbraco.CloudPurge.Cdn
{
	public interface ICdnApi : IDisposable
	{
		bool IsEnabled();
		Task<bool> HealthCheckAsync();
		Task<PurgeResponse> PurgeAsync(PurgeRequest request);
	}
}
