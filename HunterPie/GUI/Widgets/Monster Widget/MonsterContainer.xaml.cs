using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HunterPie.Core;
using Debugger = HunterPie.Logger.Debugger;

namespace HunterPie.GUI.Widgets {
    /// <summary>
    /// Interaction logic for MonsterContainer.xaml
    /// </summary>
    public partial class MonsterContainer : Widget {

        Game Context;
        MonsterHealth f_MonsterWidget;
        MonsterHealth s_MonsterWidget;
        MonsterHealth t_MonsterWidget;

        public MonsterContainer(Game ctx) {
            InitializeComponent();
            WidgetType = 1;
            SetWindowFlags();
            SetContext(ctx);
            ApplySettings();
        }

        public void SetContext(Game Ctx) {
            Context = Ctx;
            HookEvents();
        }

        private void HookEvents() {
            Context.Player.OnPeaceZoneEnter += OnPeaceZoneEnter;
            Context.Player.OnPeaceZoneLeave += OnPeaceZoneLeave;
        }

        public void UnhookEvents() {
            Context.Player.OnPeaceZoneEnter -= OnPeaceZoneEnter;
            Context.Player.OnPeaceZoneLeave -= OnPeaceZoneLeave;
            DestroyMonstersWidgets();
            this.Context = null;
        }

        private void OnPeaceZoneLeave(object source, EventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                this.WidgetHasContent = true;
                CreateMonstersWidgets();
                ChangeVisibility();
            }));
        }

        private void OnPeaceZoneEnter(object source, EventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                this.WidgetHasContent = false;
                DestroyMonstersWidgets();
                ChangeVisibility();
            }));   
        }

        private void DestroyMonstersWidgets() {
            f_MonsterWidget?.UnhookEvents();
            s_MonsterWidget?.UnhookEvents();
            t_MonsterWidget?.UnhookEvents();
            f_MonsterWidget = null;
            s_MonsterWidget = null;
            t_MonsterWidget = null;
            Container.Children.Clear();
        }

        private void CreateMonstersWidgets() {
            f_MonsterWidget = new MonsterHealth();
            s_MonsterWidget = new MonsterHealth();
            t_MonsterWidget = new MonsterHealth();
            f_MonsterWidget.SetContext(Context.FirstMonster);
            s_MonsterWidget.SetContext(Context.SecondMonster);
            t_MonsterWidget.SetContext(Context.ThirdMonster);
            Container.Children.Add(f_MonsterWidget);
            Container.Children.Add(s_MonsterWidget);
            Container.Children.Add(t_MonsterWidget);
            UpdateMonstersWidgetsSettings(UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterWeakness, UserSettings.PlayerConfig.Overlay.MonstersComponent.MaxNumberOfPartsAtOnce, UserSettings.PlayerConfig.Overlay.MonstersComponent.MonsterBarDock);
        }

        public void UpdateMonstersWidgetsSettings(bool weaknessEnabled, int MaxParts, byte dock) {
            if (f_MonsterWidget != null) {
                f_MonsterWidget.Weaknesses.Visibility = weaknessEnabled ? Visibility.Visible : Visibility.Collapsed;
                f_MonsterWidget.MonsterAilmentsContainer.MaxHeight = MaxParts * 30;
                f_MonsterWidget.MonsterPartsContainer.MaxHeight = MaxParts * 30;
                f_MonsterWidget.ChangeDocking(dock);
                f_MonsterWidget.ApplySettings();
            }
            if (s_MonsterWidget != null) {
                s_MonsterWidget.Weaknesses.Visibility = weaknessEnabled ? Visibility.Visible : Visibility.Collapsed;
                s_MonsterWidget.MonsterAilmentsContainer.MaxHeight = MaxParts * 30;
                s_MonsterWidget.MonsterPartsContainer.MaxHeight = MaxParts * 30;
                s_MonsterWidget.ChangeDocking(dock);
                s_MonsterWidget.ApplySettings();
            }
            if (t_MonsterWidget != null) {
                t_MonsterWidget.Weaknesses.Visibility = weaknessEnabled ? Visibility.Visible : Visibility.Collapsed;
                t_MonsterWidget.MonsterAilmentsContainer.MaxHeight = MaxParts * 30;
                t_MonsterWidget.MonsterPartsContainer.MaxHeight = MaxParts * 30;
                t_MonsterWidget.ChangeDocking(dock);
                t_MonsterWidget.ApplySettings();
            }
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            this.UnhookEvents();
            this.IsClosed = true;
        }

        public override void ApplySettings(bool FocusTrigger = false) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                if (!FocusTrigger) {
                    if (UserSettings.PlayerConfig.Overlay.MonstersComponent.MonsterBarDock != 1) this.Top = UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[1] + UserSettings.PlayerConfig.Overlay.Position[1];
                    this.Left = UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[0] + UserSettings.PlayerConfig.Overlay.Position[0];
                    this.WidgetActive = UserSettings.PlayerConfig.Overlay.MonstersComponent.Enabled;
                    UpdateMonstersWidgetsSettings(UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterWeakness, UserSettings.PlayerConfig.Overlay.MonstersComponent.MaxNumberOfPartsAtOnce, UserSettings.PlayerConfig.Overlay.MonstersComponent.MonsterBarDock);
                    ScaleWidget(UserSettings.PlayerConfig.Overlay.MonstersComponent.Scale, UserSettings.PlayerConfig.Overlay.MonstersComponent.Scale);
                    foreach (MonsterHealth HealthBar in this.Container.Children) {
                        HealthBar.SwitchSizeBasedOnTarget();
                    }
                }
                base.ApplySettings();
            }));
        }

        public override void EnterWidgetDesignMode() {
            base.EnterWidgetDesignMode();
            RemoveWindowTransparencyFlag();
        }

        public override void LeaveWidgetDesignMode() {
            base.LeaveWidgetDesignMode();
            ApplyWindowTransparencyFlag();
            SaveSettings();
        }

        private void SaveSettings() {
            UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
            UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
            UserSettings.PlayerConfig.Overlay.MonstersComponent.Scale = DefaultScaleX;
        }

        public void ScaleWidget(double NewScaleX, double NewScaleY) {
            if (NewScaleX <= 0.2) return;
            this.Container.LayoutTransform = new ScaleTransform(NewScaleX, NewScaleY);
            this.DefaultScaleX = NewScaleX;
            this.DefaultScaleY = NewScaleY;
        }

        private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            this.MouseOver = true;
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) {
                this.MoveWidget();
                SaveSettings();
            }
        }

        private void OnMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e) {
            if (this.MouseOver) {
                if (e.Delta > 0) {
                    ScaleWidget(DefaultScaleX + 0.05, DefaultScaleY + 0.05);
                } else {
                    ScaleWidget(DefaultScaleX - 0.05, DefaultScaleY - 0.05);
                }
            }
        }

        private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
            this.MouseOver = false;
        }

        private void OnSizeChange(object sender, SizeChangedEventArgs e) {
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.MonsterBarDock != 1) return;
            // Compensate the widget position when it's resized if the boss bar dock is bottom
            if (e.HeightChanged && !e.WidthChanged) {
                e.Handled = true;
                Top -= (e.NewSize.Height - e.PreviousSize.Height);
            }
        }
    }
}
