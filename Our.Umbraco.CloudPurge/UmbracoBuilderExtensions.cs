using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Our.Umbraco.CloudPurge.Cdn;
using Our.Umbraco.CloudPurge.CDN.CloudFlare;
using Our.Umbraco.CloudPurge.Config;
using Our.Umbraco.CloudPurge.Controllers;
using Our.Umbraco.CloudPurge.Services;
using System;
using Umbraco.Cms.Core.DependencyInjection;

namespace Our.Umbraco.CloudPurge
{
    public static class UmbracoBuilderExtensions
    {
        private const string ConfigKey = "CloudPurge";

        public static IUmbracoBuilder AddCloudPurge(this IUmbracoBuilder builder, Action<CloudPurgeConfig> defaultOptions = default)
        {
            var options = builder.Services.AddOptions<CloudPurgeConfig>()
                .Bind(builder.Config.GetSection(ConfigKey));

            if (defaultOptions != default)
            {
                options.Configure(defaultOptions);
            }

            builder.Services.AddTransient<ICdnApi, CloudFlareV4Api>();
            builder.Services.AddTransient<IContentCdnService, ContentCdnService>();
            builder.Services.AddTransient<CloudPurgeApiController>();

            //composition.Register<HttpClient>(Lifetime.Singleton);
            return builder;
        }
    }
}
