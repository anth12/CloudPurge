using System.Collections.Generic;

namespace Our.Umbraco.CloudPurge.Models
{
	public class PurgeRequest
	{
		public PurgeRequest(IEnumerable<string> urls)
		{
			Urls = urls;
		}

		/// <summary>
		/// List or absolute URL's to purge
		/// </summary>
		public IEnumerable<string> Urls { get; }
	}
}
