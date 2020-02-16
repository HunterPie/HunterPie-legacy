using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HunterPie.Core;

namespace HunterPie.GUI.Widgets {
    /// <summary>
    /// Interaction logic for MonsterHealth.xaml
    /// </summary>
    public partial class MonsterHealth : UserControl {

        private Monster Context;

        // Animations
        private Storyboard ANIM_ENRAGEDICON;
        private Storyboard ANIM_ENRAGEDBAR;

        public MonsterHealth() {
            InitializeComponent();
            this.DataContext = this;
        }

        public void SetContext(Monster ctx) {
            Context = ctx;
            HookEvents();
            LoadAnimations();
        }

        private void Dispatch(Action function) {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, function);
        }

        private void HookEvents() {
            Context.OnMonsterSpawn += OnMonsterSpawn;
            Context.OnMonsterDespawn += OnMonsterDespawn;
            Context.OnMonsterDeath += OnMonsterDespawn;
            Context.OnEnrage += OnEnrage;
            Context.OnUnenrage += OnUnenrage;
            Context.OnHPUpdate += OnMonsterUpdate;
        }

        public void UnhookEvents() {
            Context.OnMonsterSpawn -= OnMonsterSpawn;
            Context.OnMonsterDespawn -= OnMonsterDespawn;
            Context.OnMonsterDeath -= OnMonsterDespawn;
            Context.OnEnrage -= OnEnrage;
            Context.OnUnenrage -= OnUnenrage;
            Context.OnHPUpdate -= OnMonsterUpdate;
        }

        private void LoadAnimations() {
            ANIM_ENRAGEDICON = FindResource("EnragedIcon") as Storyboard;
            ANIM_ENRAGEDBAR = FindResource("EnragedHealthBar") as Storyboard;
        }

        private void OnEnrage(object source, EventArgs args) {
            this.Dispatch(() => {
                MonsterStatus.Source = (ImageSource)FindResource("ICON_ENRAGED");
                ANIM_ENRAGEDBAR.Begin(this.MonsterHPBar, true);
                ANIM_ENRAGEDICON.Begin(this.MonsterStatus, true);
            });
        }

        private void OnUnenrage(object source, EventArgs args) {
            this.Dispatch(() => {
                MonsterStatus.Source = null;
                ANIM_ENRAGEDICON.Remove(this.MonsterStatus);
                ANIM_ENRAGEDBAR.Remove(this.MonsterHPBar);
            });
        }

        private void OnMonsterDespawn(object source, EventArgs args) {
            this.Dispatch(() => {
                this.MonsterStatus.Source = null;
                this.MonsterCrown.Source = null;
                this.MonsterCrown.Visibility = Visibility.Collapsed;
                this.Visibility = Visibility.Collapsed;
                this.Weaknesses.Children.Clear();
            });
        }

        private void OnMonsterSpawn(object source, MonsterSpawnEventArgs args) {
            this.Dispatch(() => {
                this.Visibility = Visibility.Visible;
                this.MonsterName.Text = args.Name;
                this.MonsterHPBar.Value = args.CurrentHP;
                this.MonsterHPBar.Maximum = args.TotalHP;
                // Set monster crown
                this.MonsterCrown.Source = args.Crown == null ? null : (ImageSource)FindResource(args.Crown);
                this.MonsterCrown.Visibility = Visibility.Visible;
                Weaknesses.Children.Clear(); // Removes every weakness icon
                foreach (string Weakness in args.Weaknesses.Keys) {
                    Image MonsterWeaknessImg = new Image {
                        Source = this.Resources[Weakness] as ImageSource,
                        Height = 15,
                        Width = 15
                    };
                    Weaknesses.Children.Add(MonsterWeaknessImg);
                }
            });
        }

        private void OnMonsterUpdate(object source, MonsterUpdateEventArgs args) {
            this.Dispatch(() => {
                this.MonsterHPBar.Value = args.CurrentHP;
                this.MonsterHPBar.Maximum = args.TotalHP;
            });
        }
    }
}
