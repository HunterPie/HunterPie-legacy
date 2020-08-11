using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using HunterPie.Core;
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
                IsDesignModeEnabled = value;
                if (_InDesignMode) { EnterWidgetDesignMode(); }
                else { LeaveWidgetDesignMode(); }
            }
        }

        public double BaseWidth { get; set; }
        public double BaseHeight { get; set; }

        public bool IsDesignModeEnabled
        {
            get { return (bool)GetValue(IsDesignModeEnabledProperty); }
            set { SetValue(IsDesignModeEnabledProperty, value); }
        }
        public static readonly DependencyProperty IsDesignModeEnabledProperty =
            DependencyProperty.Register("IsDesignModeEnabled", typeof(bool), typeof(Widget));

        public string DesignModeDetails
        {
            get { return (string)GetValue(DesignModeDetailsProperty); }
            set { SetValue(DesignModeDetailsProperty, value); }
        }
        public static readonly DependencyProperty DesignModeDetailsProperty =
            DependencyProperty.Register("DesignModeDetails", typeof(string), typeof(Widget));

        public Visibility DesignModeDetailsVisibility
        {
            get { return (Visibility)GetValue(DesignModeDetailsVisibilityProperty); }
            set { SetValue(DesignModeDetailsVisibilityProperty, value); }
        }
        public static readonly DependencyProperty DesignModeDetailsVisibilityProperty =
            DependencyProperty.Register("DesignModeDetailsVisibility", typeof(Visibility), typeof(Widget));


        public Widget() => CompositionTarget.Rendering += OnWidgetRender;

        private int renderCounter = 0;
        private double LastFrameRender;
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
            if (InDesignMode)
            {
                RenderingEventArgs args = (RenderingEventArgs)e;
                // Dispatcher messes with the Render counter
#if !DEBUG  // Debug messes with the rendering time
                if (args.RenderingTime.TotalMilliseconds - LastFrameRender > 0)
                {

                    DesignModeDetails = $"{Left}x{Top} ({DefaultScaleX * 100:0.0}%) ({args.RenderingTime.TotalMilliseconds - LastFrameRender:0.##}ms)";
                    DesignModeDetailsVisibility = Visibility.Visible;
                    LastFrameRender = args.RenderingTime.TotalMilliseconds;

                }
#endif
            }
            else
            {
                DesignModeDetailsVisibility = Visibility.Collapsed;
            }
            
        }

        double OldOpacity;
        public virtual void EnterWidgetDesignMode()
        {
            ChangeVisibility();
            OldOpacity = Opacity;
            Opacity = 1;
        }

        public virtual void LeaveWidgetDesignMode()
        {
            ChangeVisibility();
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

            // Kinda hacky way to make it work with DirectX 11 fullscreen + Fullscreen optimizations
            if (Scanner.IsForegroundWindow && UserSettings.PlayerConfig.Overlay.EnableForceDirectX11Fullscreen)
            {
                uint GameWindowFlags = (uint)(WindowsHelper.SWP_WINDOWN_FLAGS.SWP_SHOWWINDOW |
                WindowsHelper.SWP_WINDOWN_FLAGS.SWP_NOSIZE |
                WindowsHelper.SWP_WINDOWN_FLAGS.SWP_NOMOVE);
                
                WindowsHelper.SetWindowPos(Scanner.WindowHandle, -2, 0, 0, 0, 0, GameWindowFlags);
            }
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
