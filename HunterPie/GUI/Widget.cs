using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using HunterPie.Memory;

namespace HunterPie.GUI
{

    public partial class Widget : Window
    {

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
        public bool InDesignMode
        {
            get => _InDesignMode;
            set
            {
                _InDesignMode = value;
                if (_InDesignMode) { EnterWidgetDesignMode(); }
                else { LeaveWidgetDesignMode(); }
            }
        }

        public double BaseWidth { get; set; }
        public double BaseHeight { get; set; }

        public Widget() => CompositionTarget.Rendering += OnWidgetRender;

        private int renderCounter = 0;
        private void OnWidgetRender(object sender, EventArgs e)
        {
            renderCounter++;
            if (renderCounter >= 120)
            {
                // Only force widgets on top if they are actually visible
                if (InDesignMode || (WidgetHasContent &&
                    OverlayActive &&
                    WidgetActive &&
                    ((!OverlayFocusActive) ||
                    (OverlayFocusActive && OverlayIsFocused))))
                {
                    ForceAlwaysOnTop();
                }
                renderCounter = 0;
            }
        }

        double OldOpacity;
        public virtual void EnterWidgetDesignMode()
        {
            ChangeVisibility();
            SolidColorBrush BorderColorBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ededed"));
            SolidColorBrush BackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#80e6e6e6"));
            BorderColorBrush.Freeze();
            BackgroundBrush.Freeze();
            Cursor = Cursors.SizeAll;
            BorderBrush = BorderColorBrush;
            BorderThickness = new Thickness(1, 1, 1, 1);
            Background = BackgroundBrush;
            OldOpacity = Opacity;
            Opacity = 1;
            ToolTip = $"{Left}x{Top} ({DefaultScaleX * 100:0.0}%)";
        }

        public virtual void LeaveWidgetDesignMode()
        {
            ChangeVisibility();
            BorderBrush = null;
            BorderThickness = new Thickness(0, 0, 0, 0);
            Background = Brushes.Transparent;
            Opacity = OldOpacity;
        }

        public void SetWidgetBaseSize(double Width, double Height)
        {
            BaseWidth = Width;
            BaseHeight = Height;
        }

        public void RemoveWindowTransparencyFlag()
        {
            if (IsClosed || this == null) return;

            IntPtr hwnd = new WindowInteropHelper(this).EnsureHandle();
            // Get overlay flags
            int Styles = WindowsHelper.GetWindowLong(hwnd, WindowsHelper.GWL_EXSTYLE);
            Styles &= ~(int)WindowsHelper.EX_WINDOW_STYLES.WS_EX_TRANSPARENT;
            // Apply new flags
            WindowsHelper.SetWindowLong(hwnd, WindowsHelper.GWL_EXSTYLE, Styles);
        }

        public void ApplyWindowTransparencyFlag()
        {
            if (IsClosed || this == null) return;

            IntPtr hwnd = new WindowInteropHelper(this).EnsureHandle();
            // Get overlay flags
            int Styles = WindowsHelper.GetWindowLong(hwnd, WindowsHelper.GWL_EXSTYLE);
            // Apply new flags
            WindowsHelper.SetWindowLong(hwnd, WindowsHelper.GWL_EXSTYLE, Styles | (int)WindowsHelper.EX_WINDOW_STYLES.WS_EX_TRANSPARENT);
        }

        public void SetWindowFlags()
        {
            SetWidgetBaseSize(Width, Height);

            int GWL_EXSTYLE = (-20);

            IntPtr hwnd = new WindowInteropHelper(this).EnsureHandle();
            // Get overlay flags
            int Styles = WindowsHelper.GetWindowLong(hwnd, GWL_EXSTYLE);
            int flags = (int)(WindowsHelper.EX_WINDOW_STYLES.WS_EX_TOOLWINDOW |
                WindowsHelper.EX_WINDOW_STYLES.WS_EX_TRANSPARENT |
                WindowsHelper.EX_WINDOW_STYLES.WS_EX_TOPMOST |
                WindowsHelper.EX_WINDOW_STYLES.WS_EX_NOACTIVATE);
            // Apply new flags
            WindowsHelper.SetWindowLong(hwnd, GWL_EXSTYLE, Styles | flags);
        }

        public void ForceAlwaysOnTop()
        {
            if (this == null || IsClosed) return;
            uint Flags = (uint)(WindowsHelper.SWP_WINDOWN_FLAGS.SWP_SHOWWINDOW |
                WindowsHelper.SWP_WINDOWN_FLAGS.SWP_NOSIZE |
                WindowsHelper.SWP_WINDOWN_FLAGS.SWP_NOMOVE |
                WindowsHelper.SWP_WINDOWN_FLAGS.SWP_NOACTIVATE);
            IntPtr hwnd = new WindowInteropHelper(this).EnsureHandle();
            WindowsHelper.SetWindowPos(hwnd, -1, 0, 0, 0, 0, Flags);
        }

        public virtual void ApplySettings(bool FocusTrigger = false) => ChangeVisibility();

        public virtual void MoveWidget()
        {
            DragMove();

            ToolTip = $"{Left}x{Top} ({DefaultScaleX * 100:0.0}%)";
        }

        public new void Show() => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
        {
            // Try/Catch to avoid crashes after widget is closed
            if (!IsClosed) base.Show();
        }));

        public new void Hide() => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
        {
            try
            {
                base.Hide();
            }
            catch { }
        }));

        public virtual void ChangeVisibility(bool forceOnTop = true)
        {
            if (InDesignMode || (WidgetHasContent && OverlayActive && WidgetActive && ((!OverlayFocusActive) || (OverlayFocusActive && OverlayIsFocused))))
            {
                Show();
            }
            else
            {
                Hide();
            }
            //Logger.Debugger.Log($"OverlayActive: {OverlayActive} | OverlayFocusActive: {OverlayFocusActive} | OverlayIsFocused: {OverlayIsFocused} | WidgetActive: {WidgetActive} | WidgetHasContent: {WidgetHasContent}");

        }

        public new void Close()
        {
            CompositionTarget.Rendering -= OnWidgetRender;
            base.Close();
        }

    }
}
