using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using HunterPie.Core;
using HunterPie.Memory;

namespace HunterPie.GUI {

    public partial class Widget : Window {

        public bool OverlayActive { get; set; }
        public bool OverlayFocusActive { get; set; }
        public bool OverlayIsFocused { get; set; }
        public bool WidgetActive { get; set; }
        public bool WidgetHasContent { get; set; }

        public double BaseWidth { get; set; }
        public double BaseHeight { get; set; }

        public Widget() {}

        public void SetWidgetBaseSize(double Width, double Height) {
            this.BaseWidth = Width;
            this.BaseHeight = Height;
        }

        public void SetWindowFlags(Window widget) {
            SetWidgetBaseSize(widget.Width, widget.Height);
            
            // flags to make overlay click-through
            int WS_EX_TRANSPARENT = 0x20;
            int WS_EX_TOPMOST = 0x8;
            int WS_EX_TOOLWINDOW = 0x80; // Flag to hide overlay from ALT+TAB
            int GWL_EXSTYLE = (-20);

            var wnd = GetWindow(widget);
            IntPtr hwnd = new WindowInteropHelper(wnd).EnsureHandle();
            // Get overlay flags
            int Styles = Scanner.GetWindowLong(hwnd, GWL_EXSTYLE);
            // Apply new flags
            Scanner.SetWindowLong(hwnd, GWL_EXSTYLE, Styles | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT | WS_EX_TOPMOST);
        }

        public virtual void ApplySettings() {
            ChangeVisibility();
        } 

        public virtual void MoveWidget() {}

        public new void Show() {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                base.Show();
            }));
        }

        public new void Hide() {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                base.Hide();
            }));
        }

        public virtual void ChangeVisibility() {
            if (WidgetHasContent && OverlayActive && WidgetActive && ((!OverlayFocusActive) || (OverlayFocusActive && OverlayIsFocused))) {
                this.Show();
            } else {
                this.Hide();
            }
#if DEBUG
            Logger.Debugger.Log($"OverlayActive: {OverlayActive} | OverlayFocusActive: {OverlayFocusActive} | OverlayIsFocused: {OverlayIsFocused} | WidgetActive: {WidgetActive} | WidgetHasContent: {WidgetHasContent}");
#endif
        }

    }
}
