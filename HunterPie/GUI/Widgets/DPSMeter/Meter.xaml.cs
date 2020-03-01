using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets.DPSMeter {
    /// <summary>
    /// Interaction logic for DPSMeter.xaml
    /// </summary>
    public partial class Meter : Widget {
        List<Parts.PartyMember> Players = new List<Parts.PartyMember>();
        Game GameContext;
        Party Context;

        public Meter(Game ctx) {
            InitializeComponent();
            SetWindowFlags(this);
            SetContext(ctx);
            ApplySettings();
        }

        public void SetContext(Game ctx) {
            Context = ctx.Player.PlayerParty;
            GameContext = ctx;
            HookEvents();
            this.Party.ItemsSource = Players;
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

        private void HookEvents() {
            GameContext.Player.OnPeaceZoneEnter += OnPeaceZoneEnter;
            Context.OnTotalDamageChange += OnTotalDamageChange;
            GameContext.Player.OnPeaceZoneLeave += OnPeaceZoneLeave;
        }

        private void SaveSettings() {
            UserSettings.PlayerConfig.Overlay.DPSMeter.Position[0] = (int)Left - UserSettings.PlayerConfig.Overlay.Position[0];
            UserSettings.PlayerConfig.Overlay.DPSMeter.Position[1] = (int)Top - UserSettings.PlayerConfig.Overlay.Position[1];
            UserSettings.PlayerConfig.Overlay.DPSMeter.Scale = DefaultScaleX;
        }

        private void OnPeaceZoneLeave(object source, EventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                CreatePlayerComponents();
                SortPlayersByDamage();
                ChangeVisibility();
            }));
        }

        private void OnPeaceZoneEnter(object source, EventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                DestroyPlayerComponents();
                ChangeVisibility();
            }));
        }

        public void UnhookEvents() {
            GameContext.Player.OnPeaceZoneEnter -= OnPeaceZoneEnter;
            GameContext.Player.OnPeaceZoneLeave -= OnPeaceZoneLeave;
            Context.OnTotalDamageChange -= OnTotalDamageChange;
            GameContext = null;
            Context = null;
        }

        private void OnTotalDamageChange(object source, EventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                if (Context.TotalDamage > 0) {
                    this.WidgetHasContent = true;
                    ChangeVisibility();
                }
                SortPlayersByDamage();
                if (UserSettings.PlayerConfig.Overlay.DPSMeter.ShowDPSWheneverPossible) {
                    TypeIcon.Visibility = Visibility.Visible;
                    Timer.Content = string.Format("{0:hh\\:mm\\:ss}", GameContext.Time);
                } else {
                    TypeIcon.Visibility = Visibility.Hidden;
                    Timer.Content = "Total Damage";
                }
            }));
        }

        private void CreatePlayerComponents() {
            for (int i = 0; i < Context.MaxSize; i++) {
                Parts.PartyMember pMember = new Parts.PartyMember(UserSettings.PlayerConfig.Overlay.DPSMeter.PartyMembers[i].Color);
                pMember.SetContext(Context[i], Context);
                Players.Add(pMember);
            }
            Party.Items.Refresh();
            if (Context.TotalDamage > 0) {
                WidgetHasContent = true;
                ChangeVisibility();
            }
        }

        public void DestroyPlayerComponents() {
            foreach (Parts.PartyMember player in Players) {
                player.UnhookEvents();
            }
            Players.Clear();
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                WidgetHasContent = false;
                ChangeVisibility();
            }));
        }

        private void SortPlayersByDamage() {
            Players.Sort(delegate (Parts.PartyMember x, Parts.PartyMember y) {
                return x.CompareTo(y);
            });
            Party.Items.Refresh();
        }

        public void UpdatePlayersColor() {
            if (Players.Count <= 0) return;
            for (int i = 0; i < Context.MaxSize; i++) {
                Players[i].ChangeColor(UserSettings.PlayerConfig.Overlay.DPSMeter.PartyMembers[i].Color);
            }
            Party.Items.Refresh();
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            this.UnhookEvents();
        }

        public override void ApplySettings() {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => {
                this.Top = UserSettings.PlayerConfig.Overlay.DPSMeter.Position[1] + UserSettings.PlayerConfig.Overlay.Position[1];
                this.Left = UserSettings.PlayerConfig.Overlay.DPSMeter.Position[0] + UserSettings.PlayerConfig.Overlay.Position[0];
                this.WidgetActive = UserSettings.PlayerConfig.Overlay.DPSMeter.Enabled;
                UpdatePlayersColor();
                ScaleWidget(UserSettings.PlayerConfig.Overlay.DPSMeter.Scale, UserSettings.PlayerConfig.Overlay.DPSMeter.Scale);
                base.ApplySettings();
            }));
        }
        
        public void ScaleWidget(double NewScaleX, double NewScaleY) {
            Width = BaseWidth * NewScaleX;
            Height = BaseHeight * NewScaleY;
            this.DamageContainer.LayoutTransform = new ScaleTransform(NewScaleX, NewScaleY);
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
