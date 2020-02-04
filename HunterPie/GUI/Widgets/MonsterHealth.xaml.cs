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
using System.Windows.Media.Animation;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets {
    /// <summary>
    /// Interaction logic for MonsterHealth.xaml
    /// </summary>
    public partial class MonsterHealth : UserControl {

        private Monster Context;
        
        // Animations
        private Storyboard ANIM_ENRAGE;

        public MonsterHealth() {
            InitializeComponent();
        }

        public void SetContext(Monster ctx) {
            Context = ctx;
            HookEvents();
            LoadAnimations();
        }

        private void Dispatch(Action function) {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, function);
        }

        private void HookEvents() {
            Context.OnMonsterSpawn += OnMonsterSpawn;
            Context.OnMonsterDespawn += OnMonsterDespawn;
            Context.OnMonsterDeath += OnMonsterDespawn;
            Context.OnEnrage += OnEnrage;
            Context.OnUnenrage += OnUnenrage;
            Context.OnHPUpdate += OnMonsterUpdate;
        }

        private void UnhookEvents() {
            Context.OnMonsterSpawn -= OnMonsterSpawn;
            Context.OnMonsterDespawn -= OnMonsterDespawn;
            Context.OnMonsterDeath -= OnMonsterDespawn;
            Context.OnEnrage -= OnEnrage;
            Context.OnUnenrage -= OnUnenrage;
            Context.OnHPUpdate -= OnMonsterUpdate;
        }

        private void LoadAnimations() {
            ANIM_ENRAGE = this.FindResource("Enraged") as Storyboard;
        }

        private void OnEnrage(object source, EventArgs args) {
            this.Dispatch(() => {
                ANIM_ENRAGE.Begin(this.MonsterHPBar, true);
            });
        }

        private void OnUnenrage(object source, EventArgs args) {
            this.Dispatch(() => {
                ANIM_ENRAGE.Remove(this.MonsterHPBar);
            });
        }

        private void OnMonsterDespawn(object source, MonsterEventArgs args) {
            this.Dispatch(() => {
                this.Visibility = Visibility.Collapsed;
            });
        }

        private void OnMonsterSpawn(object source, MonsterEventArgs args) {
            this.Dispatch(() => {
                this.Visibility = Visibility.Visible;
                this.MonsterName.Text = args.Name.ToUpper();
                this.MonsterHPBar.Value = args.CurrentHP;
                this.MonsterHPBar.Maximum = args.TotalHP;
                foreach (string Weakness in args.Weaknesses.Keys) {
                    Image MonsterWeaknessImg = new Image {
                        Source = this.Resources[Weakness] as ImageSource,
                        Height = 18,
                        Width = 18
                    };
                    Weaknesses.Children.Add(MonsterWeaknessImg);
                }
            });
        }

        private void OnMonsterUpdate(object source, MonsterEventArgs args) {
            this.Dispatch(() => {
                this.MonsterHPBar.Value = args.CurrentHP;
                this.MonsterHPBar.Maximum = args.TotalHP;
            });
        }
    }
}
