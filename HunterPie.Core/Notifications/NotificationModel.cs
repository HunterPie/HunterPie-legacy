namespace HunterPie.Notifications
{
    public class NotificationModel
    {
        public string Header { get; set; }
        public string Text { get; set; }
        public int Timeout { get; set; }

        public NotificationModel(string header, string text, int timeout)
        {
            Header = header;
            Text = text;
            Timeout = timeout;
        }

        public NotificationModel(string header, string text)
        {
            Header = header;
            Text = text;
            Timeout = 7000;
        }

        public NotificationModel()
        {
        }
    }
}
