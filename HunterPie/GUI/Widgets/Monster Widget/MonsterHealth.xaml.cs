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
        }

        ~MonsterHealth() {
            ANIM_ENRAGEDBAR = null;
            ANIM_ENRAGEDICON = null;
        }

        public void SetContext(Monster ctx) {
            Context = ctx;
            HookEvents();
            LoadAnimations();
            if (Context.Name != null) {
                UpdateMonsterInfo();
            } else { this.Visibility = Visibility.Collapsed; } 
        }

        private void Dispatch(Action function) {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, function);
        }

        private void HookEvents() {
            Context.OnMonsterSpawn += OnMonsterSpawn;
            Context.OnMonsterDespawn += OnMonsterDespawn;
            Context.OnMonsterDeath += OnMonsterDespawn;
            Context.OnEnrage += OnEnrage;
            Context.OnUnenrage += OnUnenrage;
            Context.OnHPUpdate += OnMonsterUpdate;
        }

        private void UpdateMonsterInfo() {
            // Used when starting HunterPie for the first time, since the events won't be triggered
            this.Visibility = Visibility.Visible;
            this.MonsterName.Text = Context.Name;
            this.MonsterHPBar.Value = Context.CurrentHP;
            this.MonsterHPBar.Maximum = Context.TotalHP;
            // Set monster crown
            this.MonsterCrown.Source = Context.Crown == null ? null : (ImageSource)FindResource(Context.Crown);
            this.MonsterCrown.Visibility = Visibility.Visible;
            Weaknesses.Children.Clear(); // Removes every weakness icon
            if (Context.Weaknesses == null) return;
            foreach (string Weakness in Context.Weaknesses.Keys) {
                Image MonsterWeaknessImg = new Image {
                    Source = this.Resources[Weakness] as ImageSource,
                    Height = 18,
                    Width = 18
                };
                Weaknesses.Children.Add(MonsterWeaknessImg);
            }
        }

        public void UnhookEvents() {
            Context.OnMonsterSpawn -= OnMonsterSpawn;
            Context.OnMonsterDespawn -= OnMonsterDespawn;
            Context.OnMonsterDeath -= OnMonsterDespawn;
            Context.OnEnrage -= OnEnrage;
            Context.OnUnenrage -= OnUnenrage;
            Context.OnHPUpdate -= OnMonsterUpdate;
            Context = null;
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
                this.MonsterName.Text = null;
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
