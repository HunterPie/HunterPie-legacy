using System;
using System.Windows;
using System.Windows.Media;
using HunterPie.Core;
using HunterPie.Core.Events;

namespace HunterPie.GUI.Widgets
{
    /// <summary>
    /// Interaction logic for MantleTimer.xaml
    /// </summary>
    public partial class MantleTimer : Widget
    {
        public new WidgetType Type => WidgetType.MantleWidget;

        private Mantle Context { get; set; }
        private int MantleNumber { get; set; }

        public float Percentage
        {
            get { return (float)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }
        public static readonly DependencyProperty PercentageProperty =
            DependencyProperty.Register("Percentage", typeof(float), typeof(MantleTimer));

        public TimeSpan Timer
        {
            get { return (TimeSpan)GetValue(TimerProperty); }
            set { SetValue(TimerProperty, value); }
        }
        public static readonly DependencyProperty TimerProperty =
            DependencyProperty.Register("Timer", typeof(TimeSpan), typeof(MantleTimer));

        public Color MantleColor
        {
            get { return (Color)GetValue(MantleColorProperty); }
            set { SetValue(MantleColorProperty, value); }
        }
        public static readonly DependencyProperty MantleColorProperty =
            DependencyProperty.Register("MantleColor", typeof(Color), typeof(MantleTimer));

        public Color MantleSecondaryColor
        {
            get { return (Color)GetValue(MantleSecondaryColorProperty); }
            set { SetValue(MantleSecondaryColorProperty, value); }
        }
        public static readonly DependencyProperty MantleSecondaryColorProperty =
            DependencyProperty.Register("MantleSecondaryColor", typeof(Color), typeof(MantleTimer));

        public bool IsCompactMode
        {
            get { return (bool)GetValue(IsCompactModeProperty); }
            set { SetValue(IsCompactModeProperty, value); }
        }
        public static readonly DependencyProperty IsCompactModeProperty =
            DependencyProperty.Register("IsCompactMode", typeof(bool), typeof(MantleTimer));


        public MantleTimer(int MantleNumber, Mantle context)
        {
            Percentage = 1;
            this.MantleNumber = MantleNumber;
            InitializeComponent();
            SetContext(context);
            SetWindowFlags();
            ApplySettings();
        }

        public override void EnterWidgetDesignMode()
        {
            base.EnterWidgetDesignMode();
            RemoveWindowTransparencyFlag();
        }

        public override void LeaveWidgetDesignMode()
        {
            base.LeaveWidgetDesignMode();
            ApplyWindowTransparencyFlag();
            SaveSettings();
        }

        private void SaveSettings()
        {
            switch (MantleNumber)
            {
                case 0:
                    UserSettings.PlayerConfig.Overlay.PrimaryMantle.Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
                    UserSettings.PlayerConfig.Overlay.PrimaryMantle.Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
                    UserSettings.PlayerConfig.Overlay.PrimaryMantle.Scale = DefaultScaleX;
                    break;
                case 1:
                    UserSettings.PlayerConfig.Overlay.SecondaryMantle.Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
                    UserSettings.PlayerConfig.Overlay.SecondaryMantle.Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
                    UserSettings.PlayerConfig.Overlay.SecondaryMantle.Scale = DefaultScaleX;
                    break;
            }

        }

        public void SetContext(Mantle ctx)
        {
            Context = ctx;
            HookEvents();
        }

        private void Dispatch(Action function) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, function);

        private void HookEvents()
        {
            Context.OnMantleCooldownUpdate += OnCooldownChange;
            Context.OnMantleTimerUpdate += OnTimerChange;
            Context.OnMantleChange += OnMantleChange;
        }

        public void UnhookEvents()
        {
            Context.OnMantleCooldownUpdate -= OnCooldownChange;
            Context.OnMantleTimerUpdate -= OnTimerChange;
            Context.OnMantleChange -= OnMantleChange;
            Context = null;
        }

        private void OnMantleChange(object source, MantleEventArgs args) => Dispatch(() =>
        {
            MantleName.Text = args.Name;
            MantleIcon.Source = GetSpecializedToolIcon(args.Id);
        });

        private void OnTimerChange(object source, MantleEventArgs args)
        {
            if (args.Timer <= 0)
            {
                Dispatch(() =>
                {
                    WidgetHasContent = false;
                    ChangeVisibility(false);
                });
                return;
            }
            string FormatMantleName = $"({(int)args.Timer}) {args.Name}";
            Dispatch(() =>
            {
                WidgetHasContent = true;
                ChangeVisibility(false);
                MantleName.Text = FormatMantleName;
                DurationBar.Width = 181 * (args.Timer / args.MaxTimer);
                Timer = TimeSpan.FromSeconds(args.Timer);
                Percentage = args.Timer / args.MaxTimer;
            });
        }

        private void OnCooldownChange(object source, MantleEventArgs args)
        {
            if (args.Cooldown <= 0)
            {
                Dispatch(() =>
                {
                    WidgetHasContent = false;
                    ChangeVisibility(false);
                });
                return;
            }
            string FormatMantleName = $"({(int)args.Cooldown}) {args.Name}";
            Dispatch(() =>
            {
                WidgetHasContent = true;
                ChangeVisibility(false);
                MantleName.Text = FormatMantleName;
                DurationBar.Width = 181 * (1 - args.Cooldown / args.MaxCooldown);
                Timer = TimeSpan.FromSeconds(args.Cooldown);
                Percentage = 1 - args.Cooldown / args.MaxCooldown;
            });
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookEvents();
            IsClosed = true;
        }

        public override void ApplySettings(bool FocusTrigger = false) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
        {
            if (!FocusTrigger)
            {
                // Changes widget position
                Top = (MantleNumber == 0 ? UserSettings.PlayerConfig.Overlay.PrimaryMantle.Position[1] : UserSettings.PlayerConfig.Overlay.SecondaryMantle.Position[1]) + UserSettings.PlayerConfig.Overlay.Position[1];
                Left = (MantleNumber == 0 ? UserSettings.PlayerConfig.Overlay.PrimaryMantle.Position[0] : UserSettings.PlayerConfig.Overlay.SecondaryMantle.Position[0]) + UserSettings.PlayerConfig.Overlay.Position[0];

                // Sets widget custom color
                Color WidgetColor = (Color)ColorConverter.ConvertFromString(MantleNumber == 0 ? UserSettings.PlayerConfig.Overlay.PrimaryMantle.Color : UserSettings.PlayerConfig.Overlay.SecondaryMantle.Color);

                MantleColor = WidgetColor;
                WidgetColor.A = 0x33;
                MantleSecondaryColor = WidgetColor;

                double ScaleFactor = MantleNumber == 0 ? UserSettings.PlayerConfig.Overlay.PrimaryMantle.Scale : UserSettings.PlayerConfig.Overlay.SecondaryMantle.Scale;
                ScaleWidget(ScaleFactor, ScaleFactor);
                // Sets visibility if enabled/disabled
                bool IsEnabled = MantleNumber == 0 ? UserSettings.PlayerConfig.Overlay.PrimaryMantle.Enabled : UserSettings.PlayerConfig.Overlay.SecondaryMantle.Enabled;
                WidgetActive = IsEnabled;

                Opacity = (MantleNumber == 0 ? UserSettings.PlayerConfig.Overlay.PrimaryMantle.Opacity : UserSettings.PlayerConfig.Overlay.SecondaryMantle.Opacity);
                IsCompactMode = (MantleNumber == 0 ? UserSettings.PlayerConfig.Overlay.PrimaryMantle.CompactMode : UserSettings.PlayerConfig.Overlay.SecondaryMantle.CompactMode);
            }
            base.ApplySettings();
        }));


        override public void ScaleWidget(double NewScaleX, double NewScaleY)
        {
            if (NewScaleX <= 0.2) return;
            Width = BaseWidth * NewScaleX;
            Height = BaseHeight * NewScaleY;
            MantleContainer.LayoutTransform = new ScaleTransform(NewScaleX, NewScaleY);
            DefaultScaleX = NewScaleX;
            DefaultScaleY = NewScaleY;
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                MoveWidget();
                SaveSettings();
            }
        }

        private void OnMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ScaleWidget(DefaultScaleX + 0.05, DefaultScaleY + 0.05);
            }
            else
            {
                ScaleWidget(DefaultScaleX - 0.05, DefaultScaleY - 0.05);
            }
        }

        // Helper

        private DrawingImage GetSpecializedToolIcon(int ID)
        {
            switch (ID)
            {
                case 0:
                    return FindResource("ICON_MANTLE_DARKGREEN") as DrawingImage;
                case 1:
                case 18:
                    return FindResource("ICON_MANTLE_YELLOW") as DrawingImage;
                case 2:
                    return FindResource("ICON_BOOSTER_GREEN") as DrawingImage;
                case 3:
                case 10:
                    return FindResource("ICON_MANTLE_GREY") as DrawingImage;
                case 4:
                case 9:
                    return FindResource("ICON_MANTLE_LIGHTGREEN") as DrawingImage;
                case 5:
                    return FindResource("ICON_MANTLE_GREEN") as DrawingImage;
                case 6:
                case 14:
                    return FindResource("ICON_MANTLE_RED") as DrawingImage;
                case 7:
                    return FindResource("ICON_MANTLE_LIGHTBLUE") as DrawingImage;
                case 8:
                case 13:
                case 16:
                    return FindResource("ICON_MANTLE") as DrawingImage;
                case 11:
                    return FindResource("ICON_BOOSTER_BLUE") as DrawingImage;
                case 12:
                    return FindResource("ICON_MANTLE_BLUE") as DrawingImage;
                case 15:
                    return FindResource("ICON_MANTLE_PURPLE") as DrawingImage;
                case 17:
                    return FindResource("ICON_BOOSTER_RED") as DrawingImage;
                case 19:
                    return FindResource("ICON_MANTLE_AC") as DrawingImage;
                default:
                    return null;
            }
        }

    }
}
