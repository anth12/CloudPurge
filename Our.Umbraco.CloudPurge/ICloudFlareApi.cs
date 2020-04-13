using System;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Models;

namespace Our.Umbraco.CloudPurge
{
	public interface ICloudFlareApi : IDisposable
	{
		Task<bool> ZoneDetailsAsync(string zoneId);
		Task<PurgeResponse> PurgeAsync(PurgeRequest request);
	}
}
