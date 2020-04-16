using System.Runtime.Serialization;

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
		public string[] IncludedContentTypes { get; set; }

		[DataMember]
		public string[] ExcludedContentTypes { get; set; }
	}
}
