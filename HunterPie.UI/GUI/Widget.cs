using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using HunterPie.Core;
using HunterPie.Memory;

namespace HunterPie.GUI
{
    public class Widget : Window
    {

        #region Variables
        private readonly Stopwatch stopwatch = new Stopwatch();

        public WidgetType Type { get; }

        public bool IsClosed { get; private set; }
        private bool inDesignMode;
        public double DefaultScaleX { get; set; } = 1;
        public double DefaultScaleY { get; set; } = 1;

        // Widget Visibility
        public bool OverlayActive { get; internal set; }
        public bool OverlayFocusActive { get; internal set; }
        public bool OverlayIsFocused { get; internal set; }
        public bool WidgetActive { get; set; }
        public bool WidgetHasContent { get; set; }

        public uint Flags { get; } =
            (uint)(WindowsHelper.SWP_WINDOWN_FLAGS.SWP_SHOWWINDOW |
                   WindowsHelper.SWP_WINDOWN_FLAGS.SWP_NOSIZE |
                   WindowsHelper.SWP_WINDOWN_FLAGS.SWP_NOMOVE |
                   WindowsHelper.SWP_WINDOWN_FLAGS.SWP_NOACTIVATE);

        public bool InDesignMode
        {
            get => inDesignMode;
            set
            {
                inDesignMode = value;
                IsDesignModeEnabled = value;
                if (inDesignMode)
                    EnterWidgetDesignMode();
                else
                    LeaveWidgetDesignMode();
            }
        }

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
        #endregion

        public Widget()
        {
            CompositionTarget.Rendering += OnWidgetRender;
            MouseWheel += OnMouseWheel;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            Initialized += OnInitialized;
            Closing += OnClosing;
        }
        
        #region Virtual

        private double oldOpacity;
        public virtual void EnterWidgetDesignMode()
        {
            ChangeVisibility();
            oldOpacity = Opacity;
        }

        public virtual void LeaveWidgetDesignMode()
        {
            ChangeVisibility();
            Opacity = oldOpacity;
            SaveSettings();
        }

        public virtual void ApplySettings()
        {
            ChangeVisibility();
        }
        public virtual void SaveSettings() {}

        public virtual void ScaleWidget(double newScaleX, double newScaleY)
        {
            if (newScaleX <= 0.2)
                return;

            LayoutTransform = new ScaleTransform(newScaleX, newScaleY);
            DefaultScaleX = newScaleX;
            DefaultScaleY = newScaleY;
        }

        public virtual void MoveWidget()
        {
            DragMove();
        }
        #endregion

        #region Public

        public void ApplyWindowTransparencyFlag()
        {
            if (IsClosed)
                return;

            IntPtr hWnd = new WindowInteropHelper(this).EnsureHandle();
            // Get widget flags
            int styles = WindowsHelper.GetWindowLong(hWnd, WindowsHelper.GWL_EXSTYLE);

            // Apply new flags
            WindowsHelper.SetWindowLong(hWnd, WindowsHelper.GWL_EXSTYLE, styles | (int)WindowsHelper.EX_WINDOW_STYLES.WS_EX_TRANSPARENT);
        }

        public void RemoveWindowTransparencyFlag()
        {
            if (IsClosed)
                return;

            IntPtr hWnd = new WindowInteropHelper(this).EnsureHandle();

            // Get widget flags
            int styles = WindowsHelper.GetWindowLong(hWnd, WindowsHelper.GWL_EXSTYLE);
            styles &= ~(int)WindowsHelper.EX_WINDOW_STYLES.WS_EX_TRANSPARENT;

            // Apply new flags
            WindowsHelper.SetWindowLong(hWnd, WindowsHelper.GWL_EXSTYLE, styles);
        }

        public void SetWindowFlags()
        {
            IntPtr hWnd = new WindowInteropHelper(this).EnsureHandle();

            int styles = WindowsHelper.GetWindowLong(hWnd, WindowsHelper.GWL_EXSTYLE);
            int flags = (int)(WindowsHelper.EX_WINDOW_STYLES.WS_EX_TOOLWINDOW |
                              WindowsHelper.EX_WINDOW_STYLES.WS_EX_TRANSPARENT |
                              WindowsHelper.EX_WINDOW_STYLES.WS_EX_TOPMOST |
                              WindowsHelper.EX_WINDOW_STYLES.WS_EX_NOACTIVATE);
            WindowsHelper.SetWindowLong(hWnd, WindowsHelper.GWL_EXSTYLE, styles | flags);
        }

        public void ChangeVisibility()
        {
            if (InDesignMode || (WidgetHasContent && OverlayActive && WidgetActive && ((!OverlayFocusActive) || (OverlayFocusActive && OverlayIsFocused))))
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        public new void Show()
        {
            Dispatch(() =>
            {
                if (!IsClosed)
                    base.Show();
            });
        }

        public new void Hide()
        {
            Dispatch(() =>
            {
                if (!IsClosed)
                    base.Hide();
            });
        }

        public new void Close()
        {
            CompositionTarget.Rendering -= OnWidgetRender;
            MouseWheel -= OnMouseWheel;
            MouseLeftButtonDown -= OnMouseLeftButtonDown;
            Closing -= OnClosing;
            Initialized -= OnInitialized;
            base.Close();
        }
        #endregion

        #region Private

        private int renderCounter = 0;
        private void OnWidgetRender(object sender, EventArgs e)
        {
            renderCounter++;

            if (renderCounter >= 120)
            {
                // Someone send me a karnaugh map please
                if (!InDesignMode &&
                    (WidgetHasContent && OverlayActive && WidgetActive &&
                    (!OverlayFocusActive || (OverlayFocusActive && OverlayIsFocused))))
                {
                    ForceAlwaysOnTop();
                }

                renderCounter = 0;
            }

            if (InDesignMode)
            {
                DesignModeDetails = $"{Left}x{Top} ({DefaultScaleX * 100:0.0}%) ({stopwatch.ElapsedMilliseconds}ms)";
                DesignModeDetailsVisibility = Visibility.Visible;
                stopwatch.Restart();
            }
            else
            {
                if (stopwatch.IsRunning)
                    stopwatch.Stop();
                DesignModeDetailsVisibility = Visibility.Collapsed;
            }
        }

        
        private void OnInitialized(object sender, EventArgs e)
        {
            ApplySettings();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            IsClosed = true;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MoveWidget();
            SaveSettings();
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double delta = 0.05 * (e.Delta > 0 ? 1 : -1);
            ScaleWidget(DefaultScaleX + delta, DefaultScaleY + delta);
        }

        private void ForceAlwaysOnTop()
        {
            if (IsClosed)
                return;

            IntPtr hWnd = new WindowInteropHelper(this).EnsureHandle();
            if (Game.IsWindowFocused && UserSettings.PlayerConfig.Overlay.EnableForceDirectX11Fullscreen)
            {
                uint gameWindowFlags = (uint)(WindowsHelper.SWP_WINDOWN_FLAGS.SWP_SHOWWINDOW |
                                              WindowsHelper.SWP_WINDOWN_FLAGS.SWP_NOMOVE);

                WindowsHelper.SetWindowPos(Kernel.WindowHandle,
                    -2, 0, 0, Screen.PrimaryScreen.Bounds.Width,
                    Screen.PrimaryScreen.Bounds.Height, gameWindowFlags);
            }

            WindowsHelper.SetWindowPos(hWnd, -1, 0, 0, 0, 0, Flags);
        }

        private void Dispatch(Action f)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, f);
        }

        #endregion
    }
}
