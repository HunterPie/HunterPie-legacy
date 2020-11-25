using System;
using System.Collections.Generic;
using System.Linq;
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

        public new WidgetType Type => WidgetType.MonsterWidget;

        Game Context;
        MonsterHealth f_MonsterWidget;
        MonsterHealth s_MonsterWidget;
        MonsterHealth t_MonsterWidget;

        double MONSTER_WIDTH_3;
        double MONSTER_WIDTH_2;
        double MONSTER_WIDTH_1;

        public MonsterContainer(Game ctx)
        {
            LoadMonsterWidths();
            InitializeComponent();
            SetWindowFlags();
            SetContext(ctx);
            ApplySettings();
        }

        private void LoadMonsterWidths()
        {
            MONSTER_WIDTH_1 = Convert.ToDouble(FindResource("OVERLAY_MONSTER_BAR_WIDTH_1"));
            MONSTER_WIDTH_2 = Convert.ToDouble(FindResource("OVERLAY_MONSTER_BAR_WIDTH_2"));
            MONSTER_WIDTH_3 = Convert.ToDouble(FindResource("OVERLAY_MONSTER_BAR_WIDTH_3"));
        }

        private void OnMonsterContainerRender(object sender, EventArgs e)
        {
            List<MonsterHealth> VisibleMonsters = Container.Children.Cast<MonsterHealth>().Where(
                component => component?.IsVisible == true).ToList();
            double newSize;
            switch (VisibleMonsters.Count())
            {
                case 1:
                    newSize = MONSTER_WIDTH_1;
                    break;
                case 2:
                    newSize = MONSTER_WIDTH_2;
                    break;
                default:
                    newSize = MONSTER_WIDTH_3;
                    break;
            }
            foreach (MonsterHealth m in VisibleMonsters)
            {
                if (m.Width != newSize)
                {
                    m.Width = newSize;
                    m.ChangeBarsSizes(newSize);
                }
            }
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
            CompositionTarget.Rendering += OnMonsterContainerRender;
        }

        public void UnhookEvents()
        {
            Context.Player.OnPeaceZoneEnter -= OnPeaceZoneEnter;
            Context.Player.OnPeaceZoneLeave -= OnPeaceZoneLeave;
            CompositionTarget.Rendering -= OnMonsterContainerRender;
            DestroyMonstersWidgets();
            Context = null;
        }

        private void OnPeaceZoneLeave(object source, EventArgs args) =>
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
        {
            WidgetHasContent = true;
            CreateMonstersWidgets();
            ChangeVisibility();
        }));

        private void OnPeaceZoneEnter(object source, EventArgs args) =>
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
        {
            WidgetHasContent = false;
            DestroyMonstersWidgets();
            ChangeVisibility();
        }));

        private void DestroyMonstersWidgets()
        {
            foreach (MonsterHealth mWidget in Container.Children)
            {
                mWidget?.UnhookEvents();
            }
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
            foreach (MonsterHealth mWidget in Container.Children)
            {
                if (mWidget != null)
                {
                    mWidget.Weaknesses.Visibility = mWidget.Weaknesses.IsEnabled && weaknessEnabled ? Visibility.Visible : Visibility.Collapsed;
                    mWidget.MonsterAilmentsContainer.MaxHeight = MaxParts * 32;
                    mWidget.MonsterPartsContainer.MaxHeight = MaxParts * 32;
                    mWidget.ChangeDocking(dock);
                    mWidget.ApplySettings();
                }
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
