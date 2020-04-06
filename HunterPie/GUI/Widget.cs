using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HunterPie.Memory;

namespace HunterPie.GUI {

    public partial class Widget : Window {

        public byte WidgetType = 0;
        public bool IsClosed = false;
        private bool _InDesignMode { get; set; }
        public double DefaultScaleX { get; set; } = 1;
        public double DefaultScaleY { get; set; } = 1;
        public bool MouseOver { get; set; }
        public bool OverlayActive { get; set; }
        public bool OverlayFocusActive { get; set; }
        public bool OverlayIsFocused { get; set; }
        public bool WidgetActive { get; set; }
        public bool WidgetHasContent { get; set; }
        public bool InDesignMode {
            get { return _InDesignMode; }
            set {
                _InDesignMode = value;
                if (_InDesignMode) { EnterWidgetDesignMode(); }
                else { LeaveWidgetDesignMode(); }
            }
        }

        public double BaseWidth { get; set; }
        public double BaseHeight { get; set; }

        public Widget() { }

        public virtual void EnterWidgetDesignMode() {
            ChangeVisibility();
            SolidColorBrush BorderColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ededed"));
            SolidColorBrush BackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#80e6e6e6"));
            BorderColorBrush.Freeze();
            BackgroundBrush.Freeze();
            this.Cursor = Cursors.SizeAll;
            this.BorderBrush = BorderColorBrush;
            this.BorderThickness = new Thickness(1, 1, 1, 1);
            this.Background = BackgroundBrush;
            this.ToolTip = $"{Left}x{Top} ({this.DefaultScaleX * 100:0.0}%)";
        }

        public virtual void LeaveWidgetDesignMode() {
            ChangeVisibility();
            this.BorderBrush = null;
            this.BorderThickness = new Thickness(0, 0, 0, 0);
            this.Background = Brushes.Transparent;
        }

        public void SetWidgetBaseSize(double Width, double Height) {
            this.BaseWidth = Width;
            this.BaseHeight = Height;
        }

        public void RemoveWindowTransparencyFlag() {
            if (this.IsClosed || this == null) return;
            int WS_EX_TRANSPARENT = 0x20;
            int GWL_EXSTYLE = (-20);

            IntPtr hwnd = new WindowInteropHelper(this).EnsureHandle();
            // Get overlay flags
            int Styles = Scanner.GetWindowLong(hwnd, GWL_EXSTYLE);
            Styles &= ~WS_EX_TRANSPARENT;
            // Apply new flags
            Scanner.SetWindowLong(hwnd, GWL_EXSTYLE, Styles);
        }

        public void ApplyWindowTransparencyFlag() {
            if (this.IsClosed || this == null) return;
            // flags to make overlay click-through
            int WS_EX_TRANSPARENT = 0x20;
            int GWL_EXSTYLE = (-20);

            IntPtr hwnd = new WindowInteropHelper(this).EnsureHandle();
            // Get overlay flags
            int Styles = Scanner.GetWindowLong(hwnd, GWL_EXSTYLE);
            // Apply new flags
            Scanner.SetWindowLong(hwnd, GWL_EXSTYLE, Styles | WS_EX_TRANSPARENT);
        }

        public void SetWindowFlags() {
            SetWidgetBaseSize(this.Width, this.Height);
            
            // flags to make overlay click-through
            int WS_EX_TRANSPARENT = 0x20;
            int WS_EX_TOPMOST = 0x8;
            int WS_EX_TOOLWINDOW = 0x80; // Flag to hide overlay from ALT+TAB
            int GWL_EXSTYLE = (-20);

            IntPtr hwnd = new WindowInteropHelper(this).EnsureHandle();
            // Get overlay flags
            int Styles = Scanner.GetWindowLong(hwnd, GWL_EXSTYLE);
            // Apply new flags
            Scanner.SetWindowLong(hwnd, GWL_EXSTYLE, Styles | WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT | WS_EX_TOPMOST);
        }

        public void ForceAlwaysOnTop() {
            if (this == null || this.IsClosed) return;
            uint SWP_SHOWWINDOW = 0x0040;
            uint SWP_NOMOVE = 0x0002;
            uint SWP_NOSIZE = 0x0001;
            uint Flags = SWP_SHOWWINDOW | SWP_NOSIZE | SWP_NOMOVE;
            IntPtr hwnd = new WindowInteropHelper(this).EnsureHandle();
            Scanner.SetWindowPos(hwnd, -1, 0, 0, 0, 0, Flags);
        }

        public virtual void ApplySettings(bool FocusTrigger = false) {
            ChangeVisibility();
        } 

        public virtual void MoveWidget() {
            this.DragMove();

            this.ToolTip = $"{Left}x{Top} ({this.DefaultScaleX * 100:0.0}%)";
        }

        public new void Show() {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                // Try/Catch to avoid crashes after widget is closed
                try {
                    base.Show();
                } catch {}
                
            }));
        }

        public new void Hide() {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                try {
                    base.Hide();
                } catch {}
            }));
        }

        public virtual void ChangeVisibility(bool forceOnTop = true) {
            if (InDesignMode || (WidgetHasContent && OverlayActive && WidgetActive && ((!OverlayFocusActive) || (OverlayFocusActive && OverlayIsFocused)))) {
                if (forceOnTop) this.ForceAlwaysOnTop();
                this.Show();
            } else {
                this.Hide();
            }
            //Logger.Debugger.Log($"OverlayActive: {OverlayActive} | OverlayFocusActive: {OverlayFocusActive} | OverlayIsFocused: {OverlayIsFocused} | WidgetActive: {WidgetActive} | WidgetHasContent: {WidgetHasContent}");

        }

    }
}
