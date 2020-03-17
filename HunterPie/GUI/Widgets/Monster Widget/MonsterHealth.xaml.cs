using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HunterPie.Core;
using HunterPie.GUIControls.Custom_Controls;

namespace HunterPie.GUI.Widgets {
    /// <summary>
    /// Interaction logic for MonsterHealth.xaml
    /// </summary>
    public partial class MonsterHealth : UserControl {

        private Monster Context;

        // Animations
        private Storyboard ANIM_ENRAGEDICON;

        public MonsterHealth() {
            InitializeComponent();
        }

        ~MonsterHealth() {
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
            Context.OnTargetted += OnMonsterTargetted;
        }

        private void UpdateMonsterInfo() {
            // Used when starting HunterPie for the first time, since the events won't be triggered
            this.Visibility = Visibility.Visible;
            this.MonsterName.Text = Context.Name;

            // Update monster health
            
            MonsterHealthBar.MaxSize = this.Width * 0.7833333333333333;
            MonsterHealthBar.UpdateBar(Context.CurrentHP, Context.TotalHP);
            SetMonsterHealthBarText(Context.CurrentHP, Context.TotalHP);

            SwitchSizeBasedOnTarget();

            // Set monster crown
            this.MonsterCrown.Source = Context.Crown == null ? null : (ImageSource)FindResource(Context.Crown);
            this.MonsterCrown.Visibility = Context.Crown == null ? Visibility.Collapsed : Visibility.Visible;
            Weaknesses.Children.Clear(); // Removes every weakness icon
            if (Context.Weaknesses == null) return;
            foreach (string Weakness in Context.Weaknesses.Keys) {
                ImageSource img = this.Resources[Weakness] as ImageSource;
                img.Freeze();
                WeaknessDisplay MonsterWeaknessDisplay = new WeaknessDisplay {
                    Icon = img,
                    Width = 20,
                    Height = 20
                };
                Weaknesses.Children.Add(MonsterWeaknessDisplay);
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
            ANIM_ENRAGEDICON = FindResource("ANIM_ENRAGED") as Storyboard;
        }

        private void OnMonsterTargetted(object source, EventArgs args) {
            Dispatch(() => {
                SwitchSizeBasedOnTarget();
            });
        }

        private void OnEnrage(object source, EventArgs args) {
            this.Dispatch(() => {
                ANIM_ENRAGEDICON.Begin(this.MonsterHealthBar, true);
                ANIM_ENRAGEDICON.Begin(this.HealthBossIcon, true);
            });
        }

        private void OnUnenrage(object source, EventArgs args) {
            this.Dispatch(() => {
                ANIM_ENRAGEDICON.Remove(this.MonsterHealthBar);
                ANIM_ENRAGEDICON.Remove(this.HealthBossIcon);
            });
        }

        private void OnMonsterDespawn(object source, EventArgs args) {
            this.Dispatch(() => {
                this.MonsterName.Text = null;
                //this.MonsterStatus.Source = null;
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

                // Update monster health
                MonsterHealthBar.MaxSize = this.Width * 0.7833333333333333;
                MonsterHealthBar.UpdateBar(Context.CurrentHP, Context.TotalHP);
                SetMonsterHealthBarText(args.CurrentHP, args.TotalHP);

                SwitchSizeBasedOnTarget();

                // Set monster crown
                this.MonsterCrown.Source = args.Crown == null ? null : (ImageSource)FindResource(args.Crown);
                this.MonsterCrown.Visibility = args.Crown == null ? Visibility.Collapsed : Visibility.Visible;
                Weaknesses.Children.Clear(); // Removes every weakness icon
                foreach (string Weakness in Context.Weaknesses.Keys) {
                    ImageSource img = this.Resources[Weakness] as ImageSource;
                    img.Freeze();
                    WeaknessDisplay MonsterWeaknessDisplay = new WeaknessDisplay {
                        Icon = img,
                        Width = 20,
                        Height = 20
                    };
                    Weaknesses.Children.Add(MonsterWeaknessDisplay);
                }
            });
        }

        private void OnMonsterUpdate(object source, MonsterUpdateEventArgs args) {
            this.Dispatch(() => {
                this.MonsterHealthBar.MaxHealth = args.TotalHP;
                this.MonsterHealthBar.Health = args.CurrentHP;
                SetMonsterHealthBarText(args.CurrentHP, args.TotalHP);
            });
        }

        private void SetMonsterHealthBarText(float hp, float max_hp) {
            this.HealthText.Text = $"{hp:0}/{max_hp:0} ({hp / max_hp * 100:0}%)";
        }

        private void SwitchSizeBasedOnTarget() {
            ShowOnlyTargetMonster();
        }
        
        // Only show one monster
        private void ShowOnlyTargetMonster() {
            if (!Context.isTarget) { this.Visibility = Visibility.Collapsed; }
            else {
                this.Visibility = Visibility.Visible;
                this.Width = 500;
                MonsterHealthBar.MaxSize = this.Width * 0.9;
                MonsterHealthBar.UpdateBar(Context.CurrentHP, Context.TotalHP);
            }
        }

        private void ShowAllButFocusTarget() {
            if (!Context.isTarget) {
                this.Width = 240;
                MonsterHealthBar.MaxSize = this.Width * 0.8;
                MonsterHealthBar.UpdateBar(Context.CurrentHP, Context.TotalHP);
                this.Opacity = 0.5;
            } else {
                this.Width = 320;
                MonsterHealthBar.MaxSize = this.Width * 0.8;
                MonsterHealthBar.UpdateBar(Context.CurrentHP, Context.TotalHP);
                this.Opacity = 1;
            }
        }

    }
}
