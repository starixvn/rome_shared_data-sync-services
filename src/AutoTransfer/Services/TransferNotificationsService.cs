using AutoTransfer.Criteria.Notifications;
using AutoTransfer.Models.Notifications;
using Fanex.Data.Repository;

namespace AutoTransfer.Services;

public interface ITransferNotificationsService
{
    Task InsertNotifications(string siteDB);
}

public sealed class TransferNotificationsService : ITransferNotificationsService
{
    private readonly IDynamicRepository dynamicRepository;

    public TransferNotificationsService(IDynamicRepository dynamicRepository)
        => this.dynamicRepository = dynamicRepository;

    public async Task InsertNotifications(string siteDB)
    {
        var failedNotifications = await GetFailedNotifications(siteDB);

        foreach (var notification in failedNotifications)
        {
            var messageId = await InsertPersonalMessage(notification);
            AssignPersonalMessage(notification, messageId);

            await InsertToastMessage(notification);

            MarkNotifiedMessage(siteDB, notification);
        }
    }

    private async Task<IEnumerable<FailedNotificationModel>> GetFailedNotifications(string siteDB)
    {
        var criteria = new GetFailedNotificationsCriteria(siteDB);

        return await dynamicRepository.FetchAsync<FailedNotificationModel>(criteria);
    }

    private async Task<long> InsertPersonalMessage(FailedNotificationModel notification)
    {
        var criteria = new InsertPersonalMessageCriteria(notification);
        var newMessages = await dynamicRepository.FetchAsync<long>(criteria);
        return newMessages.FirstOrDefault();
    }

    private void AssignPersonalMessage(FailedNotificationModel notification, long messageId)
    {
        var criteria = new AssignPersonalMessageCriteria(messageId, notification.TargetId, notification.TargetName);
        dynamicRepository.Fetch<bool>(criteria);
    }

    private async Task<long> InsertToastMessage(FailedNotificationModel notification)
    {
        var criteria = new InsertToastMessageCriteria(notification);
        var newMessages = await dynamicRepository.FetchAsync<long>(criteria);
        return newMessages.FirstOrDefault();
    }

    private void MarkNotifiedMessage(string siteDB, FailedNotificationModel notification)
    {
        var criteria = new MarkNotifiedCriteria(siteDB, notification.TargetId);
        dynamicRepository.Fetch<bool>(criteria);
    }
}
