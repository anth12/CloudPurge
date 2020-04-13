using System.Collections.Generic;

namespace Our.Umbraco.CloudPurge.Models
{
	public class PurgeRequest
	{
		public PurgeRequest(IEnumerable<string> urls, bool everything)
		{
			Urls = urls;
			Everything = everything;
		}

		/// <summary>
		/// List or absolute URL's to purge
		/// </summary>
		public IEnumerable<string> Urls { get; }

		/// <summary>
		/// Clear the entire CloudFlare cache
		/// </summary>
		public bool Everything { get; }
	}
}
