using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Our.Umbraco.CloudPurge;

public sealed class CloudPurgeComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddOptions<CloudPurgeOptions>()
            .BindConfiguration(CloudPurgeOptions.SectionName)
            .Validate(options => !string.IsNullOrWhiteSpace(options.Cloudflare.ApiToken)
                && !string.IsNullOrWhiteSpace(options.Cloudflare.ZoneId),
                "CloudPurge requires CloudPurge:Cloudflare:ApiToken and CloudPurge:Cloudflare:ZoneId.")
            .ValidateOnStart();

        builder.Services.AddHttpClient<CloudflareClient>();
        builder.Services.AddTransient<CloudPurgeService>();
        builder.AddNotificationAsyncHandler<ContentPublishingNotification, ContentPublishingHandler>();
        builder.AddNotificationAsyncHandler<ContentPublishedNotification, ContentPublishedHandler>();
        builder.AddNotificationAsyncHandler<ContentUnpublishingNotification, ContentUnpublishingHandler>();
        builder.AddNotificationAsyncHandler<ContentUnpublishedNotification, ContentUnpublishedHandler>();
        builder.AddNotificationAsyncHandler<ContentMovingNotification, ContentMovingHandler>();
        builder.AddNotificationAsyncHandler<ContentMovedNotification, ContentMovedHandler>();
        builder.AddNotificationAsyncHandler<ContentMovingToRecycleBinNotification, ContentMovingToRecycleBinHandler>();
        builder.AddNotificationAsyncHandler<ContentMovedToRecycleBinNotification, ContentMovedToRecycleBinHandler>();
    }
}
