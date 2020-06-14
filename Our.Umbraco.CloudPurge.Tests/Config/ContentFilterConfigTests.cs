
using System;
using Moq;
using NUnit.Framework;
using Our.Umbraco.CloudPurge.Config;
using Umbraco.Core.Models;

namespace Our.Umbraco.CloudPurge.Tests.Config
{
	public class ContentFilterConfigTests
	{
		[Test]
		public void AllowedContent_GivenNoConfig_ThenDoNotFilter()
		{
			var config = new ContentFilterConfig();

			var mockContentType = new Mock<IContentType>();

			var result = config.AllowedContent(mockContentType.Object);

			Assert.IsTrue(result);
		}

		#region Excluded content

		[Test]
		public void AllowedContent_GivenExcludedContentType_ThenNotAllowed()
		{
			var config = new ContentFilterConfig(Array.Empty<string>(), new []{ "mock-type" });

			var mockContentType = new Mock<IContentType>(MockBehavior.Strict);

			mockContentType.SetupProperty(c => c.Alias, "mock-type");

			var result = config.AllowedContent(mockContentType.Object);

			Assert.IsFalse(result);
		}

		[Test]
		public void AllowedContent_GivenExcludedCompositionContentType_ThenNotAllowed()
		{
			var config = new ContentFilterConfig(Array.Empty<string>(), new[] { "mock-composition-type" });

			var mockContentType = new Mock<IContentType>(MockBehavior.Strict);

			mockContentType.SetupProperty(c => c.Alias, "mock-type");
			mockContentType.Setup(c => c.CompositionAliases())
				.Returns(new[] {"mock-composition-type"});

			var result = config.AllowedContent(mockContentType.Object);

			Assert.IsFalse(result);
		}

		[Test]
		public void AllowedContent_GivenNoExcludedContentTypes_ThenAllowed()
		{
			var config = new ContentFilterConfig(Array.Empty<string>(), new[] { "other-type" });

			var mockContentType = new Mock<IContentType>(MockBehavior.Strict);

			mockContentType.SetupProperty(c => c.Alias, "mock-type");
			mockContentType.Setup(c => c.CompositionAliases())
				.Returns(Array.Empty<string>());

			var result = config.AllowedContent(mockContentType.Object);

			Assert.IsTrue(result);
		}
		#endregion

		#region Included content

		[Test]
		public void AllowedContent_GivenIncludedContentType_ThenAllowed()
		{
			var config = new ContentFilterConfig(new[] { "mock-type" }, Array.Empty<string>());

			var mockContentType = new Mock<IContentType>(MockBehavior.Strict);

			mockContentType.SetupProperty(c => c.Alias, "mock-type");

			var result = config.AllowedContent(mockContentType.Object);

			Assert.IsTrue(result);
		}

		[Test]
		public void AllowedContent_GivenIncludedCompositionContentType_ThenAllowed()
		{
			var config = new ContentFilterConfig(new[] { "mock-composition-type" }, Array.Empty<string>());

			var mockContentType = new Mock<IContentType>(MockBehavior.Strict);

			mockContentType.SetupProperty(c => c.Alias, "mock-type");
			mockContentType.Setup(c => c.CompositionAliases())
				.Returns(new[] { "mock-composition-type" });

			var result = config.AllowedContent(mockContentType.Object);

			Assert.IsTrue(result);
		}

		[Test]
		public void AllowedContent_GivenNoIncludedContentTypes_ThenNotAllowed()
		{
			var config = new ContentFilterConfig(new[] { "other-type" }, Array.Empty<string>());

			var mockContentType = new Mock<IContentType>(MockBehavior.Strict);

			mockContentType.SetupProperty(c => c.Alias, "mock-type");
			mockContentType.Setup(c => c.CompositionAliases()).Returns(Array.Empty<string>());

			var result = config.AllowedContent(mockContentType.Object);

			Assert.IsFalse(result);
		}

		#endregion
	}
}
