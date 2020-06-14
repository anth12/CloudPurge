using System;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Models;

namespace Our.Umbraco.CloudPurge.Cdn
{
	public interface ICdnApi : IDisposable
	{
		int MaxBatchSize { get; }
		bool IsEnabled();
		Task<bool> HealthCheckAsync();

		Task<PurgeResponse> PurgeByUrlAsync(PurgeRequest request);
		Task<PurgeResponse> PurgeAllAsync(PurgeAllRequest request);
	}
}
