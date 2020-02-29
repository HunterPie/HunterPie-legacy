using System;
using System.Windows;
using System.Windows.Controls;
using HunterPie.GUI;
using HunterPie.Core;
using System.Windows.Media;

namespace HunterPie.GUI.Widgets {
    /// <summary>
    /// Interaction logic for MantleTimer.xaml
    /// </summary>
    public partial class MantleTimer : Widget {
        
        private Mantle Context { get; set; }
        private int MantleNumber { get; set; }

        public MantleTimer(int MantleNumber, Mantle context) {
            this.MantleNumber = MantleNumber;
            InitializeComponent();
            this.SetContext(context);
            SetWindowFlags(this);
            ApplySettings();
        }

        public void SetContext(Mantle ctx) {
            this.Context = ctx;
            HookEvents();
        }

        private void Dispatch(Action function) {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, function);
        }

        private void HookEvents() {
            Context.OnMantleCooldownUpdate += OnCooldownChange;
            Context.OnMantleTimerUpdate += OnTimerChange;
        }

        public void UnhookEvents() {
            Context.OnMantleCooldownUpdate -= OnCooldownChange;
            Context.OnMantleTimerUpdate -= OnTimerChange;
            Context = null;
        }

        private void OnTimerChange(object source, MantleEventArgs args) {
            if (args.Timer <= 0) {
                Dispatch(() => {
                    this.WidgetHasContent = false;
                    ChangeVisibility();
                });
                return;
            }
            string FormatMantleName = $"({(int)args.Timer}) {args.Name.ToUpper()}";
            Dispatch(() => {
                this.WidgetHasContent = true;
                ChangeVisibility();
                MantleName.Content = FormatMantleName;
                MantleCooldown.Slice = args.Timer / args.staticTimer;
            });
        }

        private void OnCooldownChange(object source, MantleEventArgs args) {
            if (args.Cooldown <= 0) {
                Dispatch(() => {
                    this.WidgetHasContent = false;
                    ChangeVisibility();
                });
                return;
            }
            Dispatch(() => {
                this.WidgetHasContent = true;
                ChangeVisibility();
                string FormatMantleName = $"({(int)args.Cooldown}) {args.Name.ToUpper()}";
                MantleName.Content = FormatMantleName;
                MantleCooldown.Slice = args.Cooldown / args.staticCooldown;
            });
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            this.UnhookEvents();
        }

        public override void ApplySettings() {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                // Changes widget position
                this.Top = MantleNumber == 0 ? UserSettings.PlayerConfig.Overlay.PrimaryMantle.Position[1] : UserSettings.PlayerConfig.Overlay.SecondaryMantle.Position[1];
                this.Left = MantleNumber == 0 ? UserSettings.PlayerConfig.Overlay.PrimaryMantle.Position[0] : UserSettings.PlayerConfig.Overlay.SecondaryMantle.Position[0];

                // Sets widget custom color
                Color WidgetColor = (Color)ColorConverter.ConvertFromString(MantleNumber == 0 ? UserSettings.PlayerConfig.Overlay.PrimaryMantle.Color : UserSettings.PlayerConfig.Overlay.SecondaryMantle.Color);
                Brush WidgetColorBrush = new SolidColorBrush(WidgetColor);
                WidgetColorBrush.Freeze();
                this.MantleCooldown.Fill = DonutBrush(WidgetColor);
                this.MantleBorder.BorderBrush = WidgetColorBrush;

                // Sets visibility if enabled/disabled
                bool IsEnabled = MantleNumber == 0 ? UserSettings.PlayerConfig.Overlay.PrimaryMantle.Enabled : UserSettings.PlayerConfig.Overlay.SecondaryMantle.Enabled;
                this.WidgetActive = IsEnabled;
                base.ApplySettings();
            }));
        }

        // Helper
        private RadialGradientBrush DonutBrush(Color customColor) {
            RadialGradientBrush brush = new RadialGradientBrush {
                Center = new Point(13, 13),
                GradientOrigin = new Point(13, 13),
                MappingMode = BrushMappingMode.Absolute,
                RadiusX = 13,
                RadiusY = 13
            };
            // Add the colors to make a donut
            brush.GradientStops.Add(new GradientStop(Colors.Transparent, 0.4));
            brush.GradientStops.Add(new GradientStop(customColor, 0.4));
            brush.Freeze();
            return brush;
        }
    }
}
