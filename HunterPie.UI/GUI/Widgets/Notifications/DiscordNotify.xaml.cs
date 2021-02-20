using System;
using System.Windows;
using DiscordRPC.Message;
using Timer = System.Threading.Timer;

namespace HunterPie.GUI.Widgets.Notifications
{
    /// <summary>
    /// Interaction logic for DiscordNotify.xaml
    /// </summary>
    public partial class DiscordNotify : Widget, IDisposable
    {
        public string ImageUrl
        {
            get { return (string)GetValue(ImageUrlProperty); }
            set { SetValue(ImageUrlProperty, value); }
        }
        public static readonly DependencyProperty ImageUrlProperty =
            DependencyProperty.Register("ImageUrl", typeof(string), typeof(DiscordNotify), new PropertyMetadata("https://discord.com/assets/6debd47ed13483642cf09e832ed0bc1b.png"));

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(DiscordNotify));

        readonly JoinRequestMessage requestInfo;
        readonly Timer timeout;
        private bool disposedValue;

        public delegate void ConfirmationEvents(object source, JoinRequestMessage args);
        public event ConfirmationEvents OnRequestAccepted;
        public event ConfirmationEvents OnRequestRejected;


        public DiscordNotify(JoinRequestMessage args)
        {
            WidgetActive = true;
            WidgetHasContent = true;
            InitializeComponent();
            requestInfo = args;
            SetInformation();
            timeout = new Timer(_ => RejectRequest(), null, 15000, 0);
        }

        private void SetInformation()
        {
            Description = Core.GStrings.GetLocalizationByXPath("/Notifications/String[@ID='STATIC_DISCORD_JOIN_REQUEST']").Replace("{Username}", requestInfo.User.ToString());
        }

        private void OnAccept(object sender, RoutedEventArgs e)
        {
            Dispatch(OnRequestAccepted);
        }

        private void OnReject(object sender, RoutedEventArgs e) => RejectRequest();

        private void RejectRequest()
        {
            Dispatch(OnRequestRejected);
        }

        private void Dispatch(ConfirmationEvents e)
        {
            e?.Invoke(this, requestInfo);

            UnhookDispatchers();
            Dispose();
        }

        private void UnhookDispatchers()
        {
            foreach (ConfirmationEvents dispatcher in OnRequestAccepted.GetInvocationList())
            {
                OnRequestAccepted -= dispatcher;
            }

            foreach (ConfirmationEvents dispatcher in OnRequestRejected.GetInvocationList())
            {
                OnRequestRejected -= dispatcher;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    timeout.Dispose();
                    Close();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
