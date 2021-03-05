using System;
using System.Windows;
using System.Windows.Threading;

namespace HunterPie.Notifications
{
    public class NotificationService : INotificationsService
    {
        public static event EventHandler<NotificationModel> OnNotificationPosted;

        public static readonly Lazy<NotificationService> lazyInstance = new(() => new NotificationService());
        public static NotificationService Instance => lazyInstance.Value;

        public void AddNotification(NotificationModel model)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render,
                (Action)(() => OnNotificationPosted?.Invoke(this, model))
            );
        }
    }
}
