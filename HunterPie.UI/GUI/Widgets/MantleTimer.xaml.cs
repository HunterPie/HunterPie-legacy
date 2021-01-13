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

        private UserSettings.Config.SpecializedTool Settings
        {
            get
            {
                switch (MantleNumber)
                {
                    case 1:
                        return UserSettings.PlayerConfig.Overlay.SecondaryMantle;
                    default:
                        return UserSettings.PlayerConfig.Overlay.PrimaryMantle;
                }
            }
        }

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
            ApplyWindowTransparencyFlag();
            base.LeaveWidgetDesignMode();
        }

        public override void SaveSettings()
        {
            Settings.Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
            Settings.Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
            Settings.Scale = DefaultScaleX;

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
                    ChangeVisibility();
                });
                return;
            }
            string FormatMantleName = $"({(int)args.Timer}) {args.Name}";
            Dispatch(() =>
            {
                WidgetHasContent = true;
                ChangeVisibility();
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
                    ChangeVisibility();
                });
                return;
            }
            string FormatMantleName = $"({(int)args.Cooldown}) {args.Name}";
            Dispatch(() =>
            {
                WidgetHasContent = true;
                ChangeVisibility();
                MantleName.Text = FormatMantleName;
                DurationBar.Width = 181 * (1 - args.Cooldown / args.MaxCooldown);
                Timer = TimeSpan.FromSeconds(args.Cooldown);
                Percentage = 1 - args.Cooldown / args.MaxCooldown;
            });
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookEvents();
        }

        public override void ApplySettings()
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                // Changes widget position
                Top = Settings.Position[1] + UserSettings.PlayerConfig.Overlay.Position[1];
                Left = Settings.Position[0] + UserSettings.PlayerConfig.Overlay.Position[0];

                // Sets widget custom color
                Color widgetColor = (Color)ColorConverter.ConvertFromString(Settings.Color);

                MantleColor = widgetColor;
                widgetColor.A = 0x33;
                MantleSecondaryColor = widgetColor;

                double scaleFactor = Settings.Scale;
                ScaleWidget(scaleFactor, scaleFactor);

                // Sets visibility if enabled/disabled
                WidgetActive = Settings.Enabled;

                Opacity = Settings.Opacity;
                IsCompactMode = Settings.CompactMode;
                base.ApplySettings();
            }));
        }
            


        public override void ScaleWidget(double newScaleX, double newScaleY)
        {
            if (newScaleX <= 0.2)
                return;

            Width *= newScaleX;
            Height *= newScaleY;
            MantleContainer.LayoutTransform = new ScaleTransform(newScaleX, newScaleY);
            DefaultScaleX = newScaleX;
            DefaultScaleY = newScaleY;
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
