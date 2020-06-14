using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Models;
using Umbraco.Core.Models.PublishedContent;

namespace Our.Umbraco.CloudPurge.Services
{
	public interface IContentCdnService : IDisposable
	{
		Task<PurgeResponse> PurgeAsync(IEnumerable<IPublishedContent> content);

		Task<PurgeResponse> PurgeAsync(PurgeRequest request);

		Task<PurgeResponse> PurgeAllAsync(PurgeAllRequest request);
	}
}
