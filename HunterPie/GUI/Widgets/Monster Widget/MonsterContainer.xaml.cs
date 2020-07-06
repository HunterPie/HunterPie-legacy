using System;
using System.Windows;
using System.Windows.Media;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets
{
    /// <summary>
    /// Interaction logic for MonsterContainer.xaml
    /// </summary>
    public partial class MonsterContainer : Widget
    {

        Game Context;
        MonsterHealth f_MonsterWidget;
        MonsterHealth s_MonsterWidget;
        MonsterHealth t_MonsterWidget;

        public MonsterContainer(Game ctx)
        {
            InitializeComponent();
            WidgetType = 1;
            SetWindowFlags();
            SetContext(ctx);
            ApplySettings();
        }

        public void SetContext(Game Ctx)
        {
            Context = Ctx;
            HookEvents();
        }

        private void HookEvents()
        {
            Context.Player.OnPeaceZoneEnter += OnPeaceZoneEnter;
            Context.Player.OnPeaceZoneLeave += OnPeaceZoneLeave;
        }

        public void UnhookEvents()
        {
            Context.Player.OnPeaceZoneEnter -= OnPeaceZoneEnter;
            Context.Player.OnPeaceZoneLeave -= OnPeaceZoneLeave;
            DestroyMonstersWidgets();
            Context = null;
        }

        private void OnPeaceZoneLeave(object source, EventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
        {
            WidgetHasContent = true;
            CreateMonstersWidgets();
            ChangeVisibility();
        }));

        private void OnPeaceZoneEnter(object source, EventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
        {
            WidgetHasContent = false;
            DestroyMonstersWidgets();
            ChangeVisibility();
        }));

        private void DestroyMonstersWidgets()
        {
            f_MonsterWidget?.UnhookEvents();
            s_MonsterWidget?.UnhookEvents();
            t_MonsterWidget?.UnhookEvents();
            f_MonsterWidget = null;
            s_MonsterWidget = null;
            t_MonsterWidget = null;
            Container.Children.Clear();
        }

        private void CreateMonstersWidgets()
        {
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

        public void UpdateMonstersWidgetsSettings(bool weaknessEnabled, int MaxParts, byte dock)
        {
            if (f_MonsterWidget != null)
            {
                f_MonsterWidget.Weaknesses.Visibility = weaknessEnabled ? Visibility.Visible : Visibility.Collapsed;
                f_MonsterWidget.MonsterAilmentsContainer.MaxHeight = MaxParts * 30;
                f_MonsterWidget.MonsterPartsContainer.MaxHeight = MaxParts * 30;
                f_MonsterWidget.ChangeDocking(dock);
                f_MonsterWidget.ApplySettings();
            }
            if (s_MonsterWidget != null)
            {
                s_MonsterWidget.Weaknesses.Visibility = weaknessEnabled ? Visibility.Visible : Visibility.Collapsed;
                s_MonsterWidget.MonsterAilmentsContainer.MaxHeight = MaxParts * 30;
                s_MonsterWidget.MonsterPartsContainer.MaxHeight = MaxParts * 30;
                s_MonsterWidget.ChangeDocking(dock);
                s_MonsterWidget.ApplySettings();
            }
            if (t_MonsterWidget != null)
            {
                t_MonsterWidget.Weaknesses.Visibility = weaknessEnabled ? Visibility.Visible : Visibility.Collapsed;
                t_MonsterWidget.MonsterAilmentsContainer.MaxHeight = MaxParts * 30;
                t_MonsterWidget.MonsterPartsContainer.MaxHeight = MaxParts * 30;
                t_MonsterWidget.ChangeDocking(dock);
                t_MonsterWidget.ApplySettings();
            }
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
                Top = UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[1] + UserSettings.PlayerConfig.Overlay.Position[1];
                Left = UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[0] + UserSettings.PlayerConfig.Overlay.Position[0];
                WidgetActive = UserSettings.PlayerConfig.Overlay.MonstersComponent.Enabled;
                UpdateMonstersWidgetsSettings(UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterWeakness, UserSettings.PlayerConfig.Overlay.MonstersComponent.MaxNumberOfPartsAtOnce, UserSettings.PlayerConfig.Overlay.MonstersComponent.MonsterBarDock);
                ScaleWidget(UserSettings.PlayerConfig.Overlay.MonstersComponent.Scale, UserSettings.PlayerConfig.Overlay.MonstersComponent.Scale);
                Opacity = UserSettings.PlayerConfig.Overlay.MonstersComponent.Opacity;
                foreach (MonsterHealth HealthBar in Container.Children)
                {
                    HealthBar.SwitchSizeBasedOnTarget();
                }
            }
            base.ApplySettings();
        }));

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
            UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
            UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
            UserSettings.PlayerConfig.Overlay.MonstersComponent.Scale = DefaultScaleX;
        }

        public void ScaleWidget(double NewScaleX, double NewScaleY)
        {
            if (NewScaleX <= 0.2) return;
            Container.LayoutTransform = new ScaleTransform(NewScaleX, NewScaleY);
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

        private void OnSizeChange(object sender, SizeChangedEventArgs e)
        {
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.MonsterBarDock != 1) return;
            if (e.WidthChanged || !e.HeightChanged) return;

            double HeightDiff = e.PreviousSize.Height - e.NewSize.Height;

            Top = UserSettings.PlayerConfig.Overlay.Position[1] + UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[1] + HeightDiff;
            UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
        }

    }
}
