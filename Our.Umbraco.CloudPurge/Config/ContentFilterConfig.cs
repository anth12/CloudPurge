using System;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models;

namespace Our.Umbraco.CloudPurge.Config
{
	[DataContract]
	public class ContentFilterConfig
	{
		public ContentFilterConfig()  { }

		public ContentFilterConfig(string[] includedContentTypes, string[] excludedContentTypes)
		{
			IncludedContentTypes = includedContentTypes;
			ExcludedContentTypes = excludedContentTypes;
		}

		[DataMember]
		public string[] IncludedContentTypes { get; set; } = Array.Empty<string>();

		[DataMember]
		public string[] ExcludedContentTypes { get; set; } = Array.Empty<string>();

		public bool AllowedContent(IContentType contentType)
		{
			if (ExcludedContentTypes.Any())
			{
				if (ExcludedContentTypes.Contains(contentType.Alias) 
				    || contentType.CompositionAliases().Any(ExcludedContentTypes.Contains))
					return false;
			}

			if (IncludedContentTypes.Any())
			{
				if (IncludedContentTypes.Contains(contentType.Alias) 
				    || contentType.CompositionAliases().Any(IncludedContentTypes.Contains))
					return true;

				return false;
			}

			return true;
		}
	}
}
