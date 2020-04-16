using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Our.Umbraco.CloudPurge.Cdn;
using Our.Umbraco.CloudPurge.Models;
using Umbraco.Core.Models;

namespace Our.Umbraco.CloudPurge.Services
{
	public class ContentCdnService : IContentCdnService
	{
		private IEnumerable<ICdnApi> _cdnApis;

		public ContentCdnService(IEnumerable<ICdnApi> cdnApis)
		{
			_cdnApis = cdnApis;
		}

		public async Task PurgeAsync(IEnumerable<IContent> content)
		{

		}

		public async Task PurgeAsync(PurgeRequest request)
		{
			var purgeTasks = _cdnApis.Where(c => c.IsEnabled()).Select(cdn => cdn.PurgeAsync(request));

			var results = await Task.WhenAll(purgeTasks);
		}

		public void Dispose()
		{
			if (_cdnApis != null)
			{
				foreach (var cdnApi in _cdnApis)
				{
					cdnApi?.Dispose();
				}
			}
		}
	}
}
