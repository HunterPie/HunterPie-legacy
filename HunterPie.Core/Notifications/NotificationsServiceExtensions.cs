namespace HunterPie.Notifications
{
    public static class NotificationsServiceExtensions
    {
        public static void AddNotification(this INotificationsService notifications, string header, string text,
            int timeout = 7000)
        {
            notifications.AddNotification(new NotificationModel(header, text, timeout));
        }
    }
}
