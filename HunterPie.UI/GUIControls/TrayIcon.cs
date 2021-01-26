using System;
using System.Drawing;
using System.Windows.Forms;

namespace HunterPie.GUIControls
{
    
    public class TrayIcon : IDisposable
    {

        private readonly ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
        private readonly NotifyIcon notifyIcon = new NotifyIcon();
        private bool disposedValue = false;

        public TrayIcon(
            string tooltip,
            string text,
            Icon icon,
            MouseEventHandler doubleClickCallback)
        {
            notifyIcon.BalloonTipTitle = tooltip;
            notifyIcon.Text = text;
            notifyIcon.Icon = icon;
            notifyIcon.Visible = true;
            notifyIcon.MouseDoubleClick += doubleClickCallback;
            notifyIcon.ContextMenuStrip = contextMenuStrip;
        }

        public ToolStripMenuItem AddItem(string name)
        {
            return contextMenuStrip.Items.Add(name) as ToolStripMenuItem;
        }

        public ToolStripMenuItem AddMenuItem(string name, ToolStripMenuItem menu)
        {
            return menu.DropDownItems.Add(name) as ToolStripMenuItem;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                notifyIcon.Visible = false;
                if (disposing)
                {
                    notifyIcon.Dispose();
                    contextMenuStrip.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
