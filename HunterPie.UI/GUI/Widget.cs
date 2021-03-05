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
using HunterPie.Core.Settings;

namespace HunterPie.GUI
{
    public class Widget : Window
    {

        #region Variables
        private readonly Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// This widget type
        /// </summary>
        public virtual WidgetType Type { get; } = WidgetType.Custom;

        /// <summary>
        /// This widget settings
        /// </summary>
        public virtual IWidgetSettings Settings { get; }

        /// <summary>
        /// Whether this widget should be hidden when game is unfocused and the
        /// Hide overlay when unfocused setting is enabled
        /// </summary>
        public virtual bool ShouldHideWhenUnfocused { get; } = true;

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

        public virtual uint Flags { get; } =
            (uint)(WindowsHelper.SWP_WINDOWN_FLAGS.SWP_SHOWWINDOW |
                   WindowsHelper.SWP_WINDOWN_FLAGS.SWP_NOSIZE |
                   WindowsHelper.SWP_WINDOWN_FLAGS.SWP_NOMOVE |
                   WindowsHelper.SWP_WINDOWN_FLAGS.SWP_NOACTIVATE);

        public virtual uint RenderFlags { get; } =
            (int)(WindowsHelper.EX_WINDOW_STYLES.WS_EX_TRANSPARENT  |
                  WindowsHelper.EX_WINDOW_STYLES.WS_EX_TOPMOST      |
                  WindowsHelper.EX_WINDOW_STYLES.WS_EX_NOACTIVATE);

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
            oldOpacity = Opacity;
            ChangeVisibility();

        }

        public virtual void LeaveWidgetDesignMode()
        {
            ChangeVisibility();
            Opacity = oldOpacity;
            SaveSettings();
        }

        public virtual void ApplySettings()
        {
            if (Settings != null)
            {
                // OBS can't find windows that hide in from taskbar
                ShowInTaskbar = Settings.StreamerMode;
                Left = Settings.Position[0] + ConfigManager.Settings.Overlay.Position[0];
                Top = Settings.Position[1] + ConfigManager.Settings.Overlay.Position[1];

                ScaleWidget(Settings.Scale, Settings.Scale);

                WidgetActive = Settings.Enabled;
                Opacity = Settings.Opacity;
            }

            SetWindowFlags();
            ChangeVisibility();
        }

        public virtual void SaveSettings()
        {
            if (Settings is null)
                return;

            Settings.Position = new int[2]
            {
                (int)Left - ConfigManager.Settings.Overlay.Position[0],
                (int)Top - ConfigManager.Settings.Overlay.Position[1]
            };
            Settings.Scale = DefaultScaleX;
        }

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
            if (IsVisible)
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

            uint styles = (uint)WindowsHelper.GetWindowLong(hWnd, WindowsHelper.GWL_EXSTYLE);

            uint flags = RenderFlags;

            if (!Settings?.StreamerMode ?? true)
                flags |= (uint)WindowsHelper.EX_WINDOW_STYLES.WS_EX_TOOLWINDOW;
            else
                flags &= ~(uint)WindowsHelper.EX_WINDOW_STYLES.WS_EX_TOOLWINDOW;

            WindowsHelper.SetWindowLong(hWnd, WindowsHelper.GWL_EXSTYLE, (int)(styles | flags));
        }

        public void ChangeVisibility()
        {
            if (InDesignMode ||
                (WidgetHasContent && OverlayActive && WidgetActive &&
                ((!OverlayFocusActive) || (!ShouldHideWhenUnfocused || (OverlayFocusActive && OverlayIsFocused && ShouldHideWhenUnfocused)))))
            {
                if (Settings != null)
                    Opacity = Settings.Opacity;
                Show();
            }
            else
            {
                if (!Settings?.StreamerMode ?? true)
                    Hide();
                else
                    Opacity = 0;
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
            IsClosed = true;
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
                    (!OverlayFocusActive || (!ShouldHideWhenUnfocused || (OverlayFocusActive && OverlayIsFocused && ShouldHideWhenUnfocused)))))
                {
                    ForceAlwaysOnTop();
                }

                renderCounter = 0;
            }

            if (InDesignMode)
            {
                DesignModeDetails = $"{Left:0.0}x{Top:0.0} ({DefaultScaleX * 100:0.0}%) ({stopwatch.ElapsedMilliseconds}ms)";
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
            //ApplySettings();
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
            if (Game.IsWindowFocused && ConfigManager.Settings.Overlay.EnableForceDirectX11Fullscreen)
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
