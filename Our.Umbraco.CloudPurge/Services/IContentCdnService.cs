using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Models;
using Umbraco.Core.Models;

namespace Our.Umbraco.CloudPurge.Services
{
	public interface IContentCdnService : IDisposable
	{
		Task PurgeAsync(IEnumerable<IContent> content);

		Task PurgeAsync(PurgeRequest request);
	}
}
