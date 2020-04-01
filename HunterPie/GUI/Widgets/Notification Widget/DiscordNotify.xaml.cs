using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BitmapImage = System.Windows.Media.Imaging.BitmapImage;
using Timer = System.Threading.Timer;

namespace HunterPie.GUI.Widgets.Notification_Widget {
    /// <summary>
    /// Interaction logic for DiscordNotify.xaml
    /// </summary>
    public partial class DiscordNotify : Widget {
        DiscordRPC.Message.JoinRequestMessage RequestInfo;
        Timer Timeout;

        public delegate void ConfirmationEvents(object source, DiscordRPC.Message.JoinRequestMessage args);
        public event ConfirmationEvents OnRequestAccepted;
        public event ConfirmationEvents OnRequestRejected;


        public DiscordNotify(DiscordRPC.Message.JoinRequestMessage args) {
            this.WidgetActive = true;
            this.WidgetHasContent = true;
            InitializeComponent();
            RequestInfo = args;
            SetInformation();
            Timeout = new Timer(_ => RejectRequest(), null, 15000, 0);
        }

        ~DiscordNotify() {
            Logger.Debugger.Debug($"{this} has been collected by GC.");
        }


        private void SetInformation() {
            this.Description.Text = Core.GStrings.GetLocalizationByXPath("/Notifications/String[@ID='STATIC_DISCORD_JOIN_REQUEST']").Replace("{Username}", RequestInfo.User.ToString());
            GetProfilePicture(RequestInfo.User.GetAvatarURL(DiscordRPC.User.AvatarFormat.PNG, DiscordRPC.User.AvatarSize.x64));
        }

        private void GetProfilePicture(string AvatarURL) {
            using (var RequestAvatar = new WebClient()) {
                RequestAvatar.DownloadDataCompleted += DownloadProfilePictureComplete;
                RequestAvatar.DownloadDataAsync(new Uri(AvatarURL));
            }
        }

        private void DownloadProfilePictureComplete(object sender, DownloadDataCompletedEventArgs e) {
            if (e.Error != null) {
                Logger.Debugger.Error(e.Error);
                return;
            }
            using (var stream = new MemoryStream(e.Result)) {
                var Img = new BitmapImage();
                Img.BeginInit();
                Img.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                Img.StreamSource = stream;
                Img.EndInit();
                this.Picture.Source = Img;
                if (Picture.Source.CanFreeze) Picture.Source.Freeze();
            }
            WebClient source = sender as WebClient;
            source.DownloadDataCompleted -= DownloadProfilePictureComplete;
        }

        private void OnAccept(object sender, RoutedEventArgs e) {
            OnRequestAccepted?.Invoke(this, RequestInfo);
        }

        private void OnReject(object sender, RoutedEventArgs e) {
            RejectRequest();
        }

        private void RejectRequest() {
            OnRequestRejected?.Invoke(this, RequestInfo);
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            this.RequestInfo = null;
            Timeout.Dispose();
            Logger.Debugger.Debug("here");
            Picture.Source = null;
            Timeout = null;
        }
    }
}
