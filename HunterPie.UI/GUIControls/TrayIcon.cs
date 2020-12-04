using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace HunterPie.GUIControls
{
    
    public class TrayIcon : IDisposable
    {

        public ContextMenu ContextMenu = new ContextMenu();
        public NotifyIcon NotifyIcon = new NotifyIcon();
        private bool disposedValue = false;

        public TrayIcon() => NotifyIcon.ContextMenu = ContextMenu;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                NotifyIcon.Visible = false;
                if (disposing)
                {
                    NotifyIcon.Dispose();
                    ContextMenu.Dispose();
                    foreach (MenuItem item in ContextMenu.MenuItems) { item.Dispose(); }
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
