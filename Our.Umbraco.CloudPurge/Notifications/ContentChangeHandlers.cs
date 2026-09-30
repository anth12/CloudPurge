using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Our.Umbraco.CloudPurge;

public abstract class ContentChangeHandler(CloudPurgeService purger, IOptions<CloudPurgeOptions> options, ILogger logger)
{
    private const string OldUrlsKey = "CloudPurge.OldUrls";

    protected void Capture(StatefulNotification notification, IEnumerable<int> ids)
    {
        if (!options.Value.AutomaticPurgeEnabled)
            return;

        try
        {
            notification.State[OldUrlsKey] = purger.GetUrls(ids, true);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "CloudPurge could not capture old content URLs");
        }
    }

    protected async Task PurgeAsync(StatefulNotification notification, IEnumerable<int> ids, CancellationToken cancellationToken)
    {
        if (!options.Value.AutomaticPurgeEnabled)
            return;

        try
        {
            var oldUrls = notification.State.TryGetValue(OldUrlsKey, out var value)
                ? value as string[] ?? []
                : [];
            var newUrls = purger.GetUrls(ids, true);
            await purger.PurgeUrlsAsync(oldUrls.Concat(newUrls), cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "CloudPurge could not purge changed content");
        }
    }
}

public sealed class ContentPublishingHandler(CloudPurgeService purger, IOptions<CloudPurgeOptions> options,
    ILogger<ContentPublishingHandler> logger) : ContentChangeHandler(purger, options, logger),
    INotificationAsyncHandler<ContentPublishingNotification>
{
    public Task HandleAsync(ContentPublishingNotification notification, CancellationToken cancellationToken)
    {
        Capture(notification, notification.PublishedEntities.Select(entity => entity.Id));
        return Task.CompletedTask;
    }
}

public sealed class ContentPublishedHandler(CloudPurgeService purger, IOptions<CloudPurgeOptions> options,
    ILogger<ContentPublishedHandler> logger) : ContentChangeHandler(purger, options, logger),
    INotificationAsyncHandler<ContentPublishedNotification>
{
    public Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken)
        => PurgeAsync(notification, notification.PublishedEntities.Select(entity => entity.Id), cancellationToken);
}

public sealed class ContentUnpublishingHandler(CloudPurgeService purger, IOptions<CloudPurgeOptions> options,
    ILogger<ContentUnpublishingHandler> logger) : ContentChangeHandler(purger, options, logger),
    INotificationAsyncHandler<ContentUnpublishingNotification>
{
    public Task HandleAsync(ContentUnpublishingNotification notification, CancellationToken cancellationToken)
    {
        Capture(notification, notification.UnpublishedEntities.Select(entity => entity.Id));
        return Task.CompletedTask;
    }
}

public sealed class ContentUnpublishedHandler(CloudPurgeService purger, IOptions<CloudPurgeOptions> options,
    ILogger<ContentUnpublishedHandler> logger) : ContentChangeHandler(purger, options, logger),
    INotificationAsyncHandler<ContentUnpublishedNotification>
{
    public Task HandleAsync(ContentUnpublishedNotification notification, CancellationToken cancellationToken)
        => PurgeAsync(notification, notification.UnpublishedEntities.Select(entity => entity.Id), cancellationToken);
}

public sealed class ContentMovingHandler(CloudPurgeService purger, IOptions<CloudPurgeOptions> options,
    ILogger<ContentMovingHandler> logger) : ContentChangeHandler(purger, options, logger),
    INotificationAsyncHandler<ContentMovingNotification>
{
    public Task HandleAsync(ContentMovingNotification notification, CancellationToken cancellationToken)
    {
        Capture(notification, notification.MoveInfoCollection.Select(info => info.Entity.Id));
        return Task.CompletedTask;
    }
}

public sealed class ContentMovedHandler(CloudPurgeService purger, IOptions<CloudPurgeOptions> options,
    ILogger<ContentMovedHandler> logger) : ContentChangeHandler(purger, options, logger),
    INotificationAsyncHandler<ContentMovedNotification>
{
    public Task HandleAsync(ContentMovedNotification notification, CancellationToken cancellationToken)
        => PurgeAsync(notification, notification.MoveInfoCollection.Select(info => info.Entity.Id), cancellationToken);
}

public sealed class ContentMovingToRecycleBinHandler(CloudPurgeService purger, IOptions<CloudPurgeOptions> options,
    ILogger<ContentMovingToRecycleBinHandler> logger) : ContentChangeHandler(purger, options, logger),
    INotificationAsyncHandler<ContentMovingToRecycleBinNotification>
{
    public Task HandleAsync(ContentMovingToRecycleBinNotification notification, CancellationToken cancellationToken)
    {
        Capture(notification, notification.MoveInfoCollection.Select(info => info.Entity.Id));
        return Task.CompletedTask;
    }
}

public sealed class ContentMovedToRecycleBinHandler(CloudPurgeService purger, IOptions<CloudPurgeOptions> options,
    ILogger<ContentMovedToRecycleBinHandler> logger) : ContentChangeHandler(purger, options, logger),
    INotificationAsyncHandler<ContentMovedToRecycleBinNotification>
{
    public Task HandleAsync(ContentMovedToRecycleBinNotification notification, CancellationToken cancellationToken)
        => PurgeAsync(notification, notification.MoveInfoCollection.Select(info => info.Entity.Id), cancellationToken);
}
