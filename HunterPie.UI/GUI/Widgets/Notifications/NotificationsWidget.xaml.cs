using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media;
using HunterPie.Core;
using HunterPie.Core.Settings;
using HunterPie.GUI.Widgets.Notifications;
using HunterPie.Notifications;

namespace HunterPie.GUI.Widgets
{
    /// <summary>
    /// Interaction logic for NotificationsWidget.xaml
    /// </summary>
    public partial class NotificationsWidget : Widget
    {
        public override IWidgetSettings Settings => ConfigManager.Settings.Overlay.NotificationsWidget;

        private readonly Stack<NotificationModel> notificationsQueue = new();

        public ObservableCollection<Notification> Notifications { get; }

        private readonly int hideDelay;

        public NotificationsWidget()
        {
            Notifications = new ObservableCollection<Notification>();
            WidgetHasContent = true;
            InitializeComponent();
            SetWindowFlags();
            hideDelay = FindResource("NOTIFICATION_HIDE_DELAY") as int? ?? 300;
            NotificationService.OnNotificationPosted += NotificationService_OnNotificationPosted;
        }

        private void NotificationService_OnNotificationPosted(object sender, NotificationModel model)
        {
            if (Notifications.Count < 3)
            {
                AddImmediateNotification(model);
            }
            else
            {
                notificationsQueue.Push(model);
            }
        }

        public override void EnterWidgetDesignMode()
        {
            base.EnterWidgetDesignMode();
            RemoveWindowTransparencyFlag();
        }

        public override void LeaveWidgetDesignMode()
        {
            ApplyWindowTransparencyFlag();
            base.LeaveWidgetDesignMode();
        }

        public override void ScaleWidget(double newScaleX, double newScaleY)
        {
            if (newScaleX <= 0.2)
                return;

            Container.LayoutTransform = new ScaleTransform(newScaleX, newScaleY);
            DefaultScaleX = newScaleX;
            DefaultScaleY = newScaleY;
        }

        private void AddImmediateNotification(NotificationModel model)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                var notif = new Notification(model);
                notif.OnShouldBeShownChanged += Notif_ShouldBeShownChanged;
                Notifications.Add(notif);
            }));
        }

        private async void Notif_ShouldBeShownChanged(Notification notif, bool isVisible)
        {
            // wait for animation to end
            await Task.Delay(hideDelay);
            Notifications.Remove(notif);
            notif.OnShouldBeShownChanged -= Notif_ShouldBeShownChanged;

            if (notificationsQueue.Count > 0)
            {
                var model = notificationsQueue.Pop();
                AddImmediateNotification(model);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            NotificationService.OnNotificationPosted -= NotificationService_OnNotificationPosted;
        }
    }
}
