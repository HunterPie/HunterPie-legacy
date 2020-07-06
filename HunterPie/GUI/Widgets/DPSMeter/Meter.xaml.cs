using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets.DPSMeter
{
    /// <summary>
    /// Interaction logic for DPSMeter.xaml
    /// </summary>
    public partial class Meter : Widget
    {
        List<Parts.PartyMember> Players = new List<Parts.PartyMember>();
        Game GameContext;
        Party Context;

        public Visibility TimerVisibility
        {
            get => (Visibility)GetValue(TimerVisibilityProperty);
            set => SetValue(TimerVisibilityProperty, value);
        }

        public static readonly DependencyProperty TimerVisibilityProperty =
            DependencyProperty.Register("TimerVisibility", typeof(Visibility), typeof(Meter));

        public Meter(Game ctx)
        {
            InitializeComponent();
            SetWindowFlags();
            WidgetType = 4;
            SetContext(ctx);
            ApplySettings();
        }

        private void OnMeterRender(object sender, EventArgs e)
        {
            if (Context == null) return;
            Timer.Text = Context.Epoch.ToString(@"hh\:mm\:ss\.ff");
        }

        public void SetContext(Game ctx)
        {
            Context = ctx.Player.PlayerParty;
            GameContext = ctx;
            HookEvents();
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

        private void HookEvents()
        {
            GameContext.Player.OnPeaceZoneEnter += OnPeaceZoneEnter;
            Context.OnTotalDamageChange += OnTotalDamageChange;
            GameContext.Player.OnPeaceZoneLeave += OnPeaceZoneLeave;
        }

        private void SaveSettings()
        {
            UserSettings.PlayerConfig.Overlay.DPSMeter.Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
            UserSettings.PlayerConfig.Overlay.DPSMeter.Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
            UserSettings.PlayerConfig.Overlay.DPSMeter.Scale = DefaultScaleX;
        }

        private void OnPeaceZoneLeave(object source, EventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
        {
            CompositionTarget.Rendering += OnMeterRender;
            CreatePlayerComponents();
            SortPlayersByDamage();
            if (UserSettings.PlayerConfig.Overlay.DPSMeter.ShowTimerInExpeditions)
            {
                if (Context == null || Context.TotalDamage <= 0) Party.Visibility = Visibility.Collapsed;
                WidgetHasContent = true;
            }
            ChangeVisibility();
        }));

        private void OnPeaceZoneEnter(object source, EventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
        {
            CompositionTarget.Rendering -= OnMeterRender;
            Timer.Text = "";
            DestroyPlayerComponents();
            ChangeVisibility();
        }));

        public void UnhookEvents()
        {
            CompositionTarget.Rendering -= OnMeterRender;
            GameContext.Player.OnPeaceZoneEnter -= OnPeaceZoneEnter;
            GameContext.Player.OnPeaceZoneLeave -= OnPeaceZoneLeave;
            Context.OnTotalDamageChange -= OnTotalDamageChange;
            Party.Children.Clear();
            foreach (Parts.PartyMember player in Players)
            {
                player.UnhookEvents();
            }
            Players.Clear();
            Players = null;
            GameContext = null;
            Context = null;
        }

        private void OnTotalDamageChange(object source, EventArgs args) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
        {
            if (Context.TotalDamage > 0 && !UserSettings.PlayerConfig.Overlay.DPSMeter.ShowOnlyTimer)
            {
                Party.Visibility = Visibility.Visible;
                WidgetHasContent = true;
                ChangeVisibility(false);
            }
            else
            {
                WidgetHasContent = UserSettings.PlayerConfig.Overlay.DPSMeter.ShowTimerInExpeditions;
                Party.Visibility = Visibility.Collapsed;
                ChangeVisibility(false);
            }
            SortPlayersByDamage();
        }));

        private void CreatePlayerComponents()
        {
            for (int i = 0; i < Context.MaxSize; i++)
            {
                Parts.PartyMember pMember = new Parts.PartyMember(UserSettings.PlayerConfig.Overlay.DPSMeter.PartyMembers[i].Color);
                pMember.SetContext(Context[i], Context);
                Players.Add(pMember);
            }
            foreach (Parts.PartyMember Member in Players)
            {
                Party.Children.Add(Member);
            }
            if (Context.TotalDamage > 0)
            {
                WidgetHasContent = true;
                ChangeVisibility();
            }
        }

        public void DestroyPlayerComponents()
        {
            foreach (Parts.PartyMember player in Players)
            {
                player?.UnhookEvents();
            }
            Players.Clear();
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
            {
                WidgetHasContent = false;
                ChangeVisibility();
                Party.Children.Clear();
            }));
        }

        private void SortPlayersByDamage()
        {
            foreach (Parts.PartyMember Player in Players)
            {
                Player.UpdateDamage();
            }
            Party.Children.Clear();
            foreach (Parts.PartyMember Player in Players.OrderByDescending(player => player.Context?.Damage))
            {
                Party.Children.Add(Player);
            }
        }

        public void UpdatePlayersColor()
        {
            if (Players == null || Players?.Count <= 0) return;
            for (int i = 0; i < Context.MaxSize; i++)
            {
                Players[i].ChangeColor(UserSettings.PlayerConfig.Overlay.DPSMeter.PartyMembers[i].Color);
                Players[i].UpdateDamageTextSettings();
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
                Top = UserSettings.PlayerConfig.Overlay.DPSMeter.Position[1] + UserSettings.PlayerConfig.Overlay.Position[1];
                Left = UserSettings.PlayerConfig.Overlay.DPSMeter.Position[0] + UserSettings.PlayerConfig.Overlay.Position[0];
                WidgetActive = UserSettings.PlayerConfig.Overlay.DPSMeter.Enabled;

                TimerVisibility = UserSettings.PlayerConfig.Overlay.DPSMeter.ShowTimer ? Visibility.Visible : Visibility.Collapsed;

                UpdatePlayersColor();
                ScaleWidget(UserSettings.PlayerConfig.Overlay.DPSMeter.Scale, UserSettings.PlayerConfig.Overlay.DPSMeter.Scale);
                Opacity = UserSettings.PlayerConfig.Overlay.DPSMeter.Opacity;
                Color PartyBgColor = new Color()
                {
                    R = 0x00,
                    G = 0x00,
                    B = 0x00,
                    A = (byte)(int)(UserSettings.PlayerConfig.Overlay.DPSMeter.BackgroundOpacity * 0xFF)
                };
                SolidColorBrush brush = new SolidColorBrush(PartyBgColor);
                brush.Freeze();
                DamageContainer.Background = brush;
                Party.Visibility = UserSettings.PlayerConfig.Overlay.DPSMeter.ShowOnlyTimer ? Visibility.Collapsed : Visibility.Visible;
                if (UserSettings.PlayerConfig.Overlay.DPSMeter.ShowTimerInExpeditions)
                {
                    if (Context == null || Context.TotalDamage <= 0) Party.Visibility = Visibility.Collapsed;
                    WidgetHasContent = true;
                }
                if (Context != null) WidgetHasContent = UserSettings.PlayerConfig.Overlay.DPSMeter.ShowTimerInExpeditions && !GameContext.Player.InPeaceZone;
                if (Context != null) SortPlayersByDamage();
            }
            base.ApplySettings();
        }));

        public void ScaleWidget(double NewScaleX, double NewScaleY)
        {
            if (NewScaleX <= 0.2) return;
            Width = BaseWidth * NewScaleX;
            Height = BaseHeight * NewScaleY;
            DamageContainer.LayoutTransform = new ScaleTransform(NewScaleX, NewScaleY);
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
    }
}
