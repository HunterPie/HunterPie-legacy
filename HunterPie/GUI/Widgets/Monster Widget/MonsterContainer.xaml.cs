using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HunterPie.Core;

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
            SetWindowFlags(this);
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
            UpdateMonstersWidgetsSettings(UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterWeakness);
        }

        public void UpdateMonstersWidgetsSettings(bool weaknessEnabled) {
            if (f_MonsterWidget != null) f_MonsterWidget.Weaknesses.Visibility = weaknessEnabled ? Visibility.Visible : Visibility.Collapsed;
            if (s_MonsterWidget != null) s_MonsterWidget.Weaknesses.Visibility = weaknessEnabled ? Visibility.Visible : Visibility.Collapsed;
            if (t_MonsterWidget != null) t_MonsterWidget.Weaknesses.Visibility = weaknessEnabled ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            this.UnhookEvents();
        }

        public override void ApplySettings() {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                this.Top = UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[1] + UserSettings.PlayerConfig.Overlay.Position[1];
                this.Left = UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[0] + UserSettings.PlayerConfig.Overlay.Position[0];
                this.WidgetActive = UserSettings.PlayerConfig.Overlay.MonstersComponent.Enabled;
                UpdateMonstersWidgetsSettings(UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterWeakness);
                ScaleWidget(UserSettings.PlayerConfig.Overlay.MonstersComponent.Scale, UserSettings.PlayerConfig.Overlay.MonstersComponent.Scale);
                base.ApplySettings();
            }));
        }

        public override void EnterWidgetDesignMode() {
            base.EnterWidgetDesignMode();
            RemoveWindowTransparencyFlag(this);
        }

        public override void LeaveWidgetDesignMode() {
            base.LeaveWidgetDesignMode();
            ApplyWindowTransparencyFlag(this);
            SaveSettings();
        }

        private void SaveSettings() {
            UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
            UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
            UserSettings.PlayerConfig.Overlay.MonstersComponent.Scale = DefaultScaleX;
        }

        public void ScaleWidget(double NewScaleX, double NewScaleY) {
            Width = BaseWidth * NewScaleX;
            Height = BaseHeight * NewScaleY;
            this.Container.LayoutTransform = new ScaleTransform(NewScaleX, NewScaleY);
            this.DefaultScaleX = NewScaleX;
            this.DefaultScaleY = NewScaleY;
        }

        private void OnMouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
            this.MouseOver = true;
        }

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            this.MoveWidget();
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

    }
}
