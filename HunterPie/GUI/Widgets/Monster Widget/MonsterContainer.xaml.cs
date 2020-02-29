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
                CreateMonstersWidgets();
            }));
        }

        private void OnPeaceZoneEnter(object source, EventArgs args) {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() => {
                DestroyMonstersWidgets();
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
                this.Top = UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[1];
                this.Left = UserSettings.PlayerConfig.Overlay.MonstersComponent.Position[0];
                this.Visibility = UserSettings.PlayerConfig.Overlay.MonstersComponent.Enabled ? Visibility.Visible : Visibility.Hidden;
                
            }));
        }

    }
}
