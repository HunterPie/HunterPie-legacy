using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HunterPie.Core;
using HunterPie.GUIControls.Custom_Controls;
using BitmapImage = System.Windows.Media.Imaging.BitmapImage;
using Timer = System.Threading.Timer;

namespace HunterPie.GUI.Widgets
{
    /// <summary>
    /// Interaction logic for MonsterHealth.xaml
    /// </summary>
    public partial class MonsterHealth : UserControl
    {

        private Monster Context;
        private Timer VisibilityTimer;

        // Animations
        private Storyboard ANIM_ENRAGEDICON;

        public MonsterHealth() => InitializeComponent();

        ~MonsterHealth()
        {
            ANIM_ENRAGEDICON = null;
        }

        public void SetContext(Monster ctx)
        {
            Context = ctx;
            HookEvents();
            LoadAnimations();
            Visibility = Visibility.Collapsed;
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode == (byte)3)
            {
                StartVisibilityTimer();
            }
            if (Context.IsActuallyAlive)
            {
                UpdateMonsterInfo(Context);
            }
        }

        private void Dispatch(Action function) => Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, function);

        private void LoadAnimations() => ANIM_ENRAGEDICON = FindResource("ANIM_ENRAGED") as Storyboard;

        private void HookEvents()
        {
            Context.OnMonsterSpawn += OnMonsterSpawn;
            Context.OnMonsterDespawn += OnMonsterDespawn;
            Context.OnMonsterDeath += OnMonsterDespawn;
            Context.OnHPUpdate += OnMonsterUpdate;
            Context.OnStaminaUpdate += OnStaminaUpdate;
            Context.OnEnrage += OnEnrage;
            Context.OnUnenrage += OnUnenrage;
            Context.OnEnrageTimerUpdate += OnEnrageTimerUpdate;
            Context.OnTargetted += OnMonsterTargetted;
            Context.OnCrownChange += OnMonsterCrownChange;
        }

        public void UnhookEvents()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                foreach (Monster_Widget.Parts.MonsterPart Part in MonsterPartsContainer.Children)
                {
                    Part.UnhookEvents();
                }
                foreach (Monster_Widget.Parts.MonsterAilment Ailment in MonsterAilmentsContainer.Children)
                {
                    Ailment.UnhookEvents();
                }
                MonsterAilmentsContainer.Children.Clear();
                MonsterPartsContainer.Children.Clear();
                Context.ClearParts();
            }));
            Context.OnMonsterSpawn -= OnMonsterSpawn;
            Context.OnMonsterDespawn -= OnMonsterDespawn;
            Context.OnMonsterDeath -= OnMonsterDespawn;
            Context.OnHPUpdate -= OnMonsterUpdate;
            Context.OnStaminaUpdate -= OnStaminaUpdate;
            Context.OnEnrage -= OnEnrage;
            Context.OnUnenrage -= OnUnenrage;
            Context.OnEnrageTimerUpdate -= OnEnrageTimerUpdate;
            Context.OnTargetted -= OnMonsterTargetted;
            Context.OnCrownChange -= OnMonsterCrownChange;
            Context = null;
        }

        #region Monster Events
        private void UpdateMonsterInfo(Monster Monster)
        {
            // Used when starting HunterPie for the first time, since the events won't be triggered
            Visibility = Visibility.Visible;
            MonsterName.Text = Monster.Name;
            // Update monster health
            MonsterHealthBar.MaxSize = Width * 0.7833333333333333;
            MonsterHealthBar.UpdateBar(Monster.CurrentHP, Monster.TotalHP);
            SetMonsterHealthBarText(Monster.CurrentHP, Monster.TotalHP);

            if ((Monster.CurrentHP / Monster.TotalHP * 100) < Monster.CaptureThreshold) CapturableIcon.Visibility = Visibility.Visible;

            // Monster stamina
            MonsterStaminaBar.MaxSize = Width - 72;
            MonsterStaminaBar.UpdateBar(Monster.Stamina, Monster.MaxStamina);
            SetMonsterStaminaText(Monster.Stamina, Monster.MaxStamina);

            // Gets monster icon
            MonsterIcon.Source = GetMonsterIcon(Monster.Id);

            SwitchSizeBasedOnTarget();

            // Parts
            int index = 0;
            MonsterPartsContainer.Children.Clear();
            while (index < Monster.Parts.Count)
            {
                Part mPart = Monster.Parts[index];
                Monster_Widget.Parts.MonsterPart PartDisplay = new Monster_Widget.Parts.MonsterPart()
                {
                    Style = FindResource("OVERLAY_MONSTER_PART_BAR_STYLE") as Style
                };
                PartDisplay.SetContext(mPart, MonsterPartsContainer.ItemWidth);
                MonsterPartsContainer.Children.Add(PartDisplay);
                index++;
            }

            // Ailments
            index = 0;
            MonsterAilmentsContainer.Children.Clear();
            while (index < Monster.Ailments.Count)
            {
                Ailment ailment = Monster.Ailments[index];
                Monster_Widget.Parts.MonsterAilment AilmentDisplay = new Monster_Widget.Parts.MonsterAilment()
                {
                    Style = FindResource("OVERLAY_MONSTER_AILMENT_BAR_STYLE") as Style
                };
                AilmentDisplay.SetContext(ailment, MonsterAilmentsContainer.ItemWidth);
                MonsterAilmentsContainer.Children.Add(AilmentDisplay);
                index++;
            }

            // Enrage
            if (Monster.IsEnraged)
            {
                ANIM_ENRAGEDICON.Begin(MonsterHealthBar, true);
                ANIM_ENRAGEDICON.Begin(HealthBossIcon, true);
                EnrageTimerText.Visibility = Visibility.Visible;
                EnrageTimerText.Text = $"{Monster.EnrageTimerStatic - Monster.EnrageTimer:0}s";
            }

            // Set monster crown
            MonsterCrown.Source = Monster.Crown == null ? null : (ImageSource)FindResource(Monster.Crown);
            MonsterCrown.Visibility = Monster.Crown == null ? Visibility.Collapsed : Visibility.Visible;
            Weaknesses.Children.Clear(); // Removes every weakness icon
            if (Monster.Weaknesses == null) return;
            foreach (string Weakness in Monster.Weaknesses.Keys)
            {
                ImageSource img = FindResource(Weakness) as ImageSource;
                img?.Freeze();
                WeaknessDisplay MonsterWeaknessDisplay = new WeaknessDisplay
                {
                    Icon = img,
                    Width = 20,
                    Height = 20
                };
                Weaknesses.Children.Add(MonsterWeaknessDisplay);
            }
        }

        private void OnMonsterCrownChange(object source, EventArgs args) => Dispatch(() =>
        {
            Logger.Debugger.Debug($"[Monster Widget] Updated crown for {Name} -> {Context.Crown}");
            MonsterCrown.Source = Context.Crown == null ? null : (ImageSource)FindResource(Context.Crown);
            MonsterCrown.Visibility = Context.Crown == null ? Visibility.Collapsed : Visibility.Visible;
        });

        private void OnMonsterTargetted(object source, EventArgs args) => Dispatch(() =>
        {
            SwitchSizeBasedOnTarget();
        });

        private void OnEnrage(object source, MonsterUpdateEventArgs args) => Dispatch(() =>
        {
            ANIM_ENRAGEDICON.Begin(MonsterHealthBar, true);
            ANIM_ENRAGEDICON.Begin(HealthBossIcon, true);
        });

        private void OnEnrageTimerUpdate(object source, MonsterUpdateEventArgs args)
        {
            if (Context == null) return;
            int EnrageTimer = (int)Context.EnrageTimerStatic - (int)Context.EnrageTimer;
            Dispatch(() =>
            {
                if (Context == null) return;
                EnrageTimerText.Visibility = Context.EnrageTimer > 0 && Context.EnrageTimer <= Context.EnrageTimerStatic ? Visibility.Visible : Visibility.Hidden;
                EnrageTimerText.Text = $"{EnrageTimer}";
            });
        }

        private void OnStaminaUpdate(object source, MonsterUpdateEventArgs args) => Dispatch(() =>
        {
            MonsterStaminaBar.UpdateBar(args.Stamina, args.MaxStamina);
            SetMonsterStaminaText(args.Stamina, args.MaxStamina);
        });

        private void OnUnenrage(object source, MonsterUpdateEventArgs args) => Dispatch(() =>
        {
            ANIM_ENRAGEDICON.Remove(MonsterHealthBar);
            ANIM_ENRAGEDICON.Remove(HealthBossIcon);
        });

        private void OnMonsterDespawn(object source, EventArgs args) => Dispatch(() =>
        {
            MonsterCrown.Visibility = Visibility.Collapsed;
            Visibility = Visibility.Collapsed;
            Weaknesses.Children.Clear();
            foreach (Monster_Widget.Parts.MonsterPart Part in MonsterPartsContainer.Children)
            {
                Part.UnhookEvents();
            }
            foreach (Monster_Widget.Parts.MonsterAilment Ailment in MonsterAilmentsContainer.Children)
            {
                Ailment.UnhookEvents();
            }
            MonsterAilmentsContainer.Children.Clear();
            MonsterPartsContainer.Children.Clear();
            Context?.ClearParts();
        });

        private void OnMonsterSpawn(object source, MonsterSpawnEventArgs args) => Dispatch(() =>
        {
            UpdateMonsterInfo(Context);
        });

        private void OnMonsterUpdate(object source, MonsterUpdateEventArgs args) => Dispatch(() =>
        {
            MonsterHealthBar.MaxHealth = args.TotalHP;
            MonsterHealthBar.Health = args.CurrentHP;
            SetMonsterHealthBarText(args.CurrentHP, args.TotalHP);
            if ((args.CurrentHP / args.TotalHP * 100) < Context.CaptureThreshold) CapturableIcon.Visibility = Visibility.Visible;
            else { CapturableIcon.Visibility = Visibility.Collapsed; }
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode == (byte)3)
            {
                Visibility = Visibility.Visible;
                StartVisibilityTimer();
            }
        });
        #endregion


        #region Visibility timer
        private void StartVisibilityTimer()
        {
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode != (byte)3)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
                {
                    Visibility = Visibility.Visible;
                }));
                return;
            }
            if (VisibilityTimer == null)
            {
                VisibilityTimer = new Timer(_ => HideUnactiveBar(), null, 10, 0);
            }
            else
            {
                VisibilityTimer.Change(15000, 0);
            }
        }

        private void HideUnactiveBar()
        {
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode != (byte)3)
            {
                return;
            }
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                Visibility = Visibility.Collapsed;
            }));
        }

        #endregion

        #region Monster bar modes
        public void SwitchSizeBasedOnTarget()
        {
            switch (UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode)
            {
                case 0: // Default
                    ShowAllMonstersAtOnce();
                    break;
                case 1: // Show all but highlight target
                    ShowAllButFocusTarget();
                    break;
                case 2: // Show only target
                    ShowOnlyTargetMonster();
                    break;
                case 3: // Show all but hide unactive monsters
                    ShowAllMonsterAndHideUnactive();
                    break;
                case 4: // Show all or only selected monster
                    ShowAllOrSelected();
                    break;
            }
        }

        // Show all monsters or the selected one (if any)
        private void ShowAllOrSelected()
        {
            if (Context == null || !Context.IsAlive) { Visibility = Visibility.Collapsed; return; }
            else { Visibility = Visibility.Visible; }
            if (Context.IsSelect == 1)
            { // this monster selected
                Width = 500;
                ChangeBarsSizes(Width);
                Opacity = 1;
            }
            else if (Context.IsSelect == 0)
            { // nothing selected, show all
                Width = 300;
                ChangeBarsSizes(Width);
                Opacity = 1;
            }
            else { Visibility = Visibility.Collapsed; } // another monster selected, hide this monster
        }


        // Show all monsters but hide unactive
        private void ShowAllMonsterAndHideUnactive()
        {
            if (Context == null || !Context.IsAlive) { Visibility = Visibility.Collapsed; return; }
            StartVisibilityTimer();
            Width = 300;
            ChangeBarsSizes(Width);
            Opacity = 1;
        }

        // Show all monsters at once
        private void ShowAllMonstersAtOnce()
        {
            if (Context != null && Context.IsAlive) Visibility = Visibility.Visible;
            else { Visibility = Visibility.Collapsed; return; }
            Width = 300;
            ChangeBarsSizes(Width);
            Opacity = 1;
        }

        // Only show one monster
        private void ShowOnlyTargetMonster()
        {
            if (Context == null || !Context.IsTarget || !Context.IsAlive) { Visibility = Visibility.Collapsed; return; }
            else
            {
                Visibility = Visibility.Visible;
                Width = 500;
                ChangeBarsSizes(Width);
                Opacity = 1;

            }
        }

        // Show all monsters but highlight only target
        private void ShowAllButFocusTarget()
        {
            if (Context != null && Context.IsAlive) Visibility = Visibility.Visible;
            else { Visibility = Visibility.Collapsed; return; }
            if (!Context.IsTarget)
            {
                Width = 240;
                ChangeBarsSizes(Width);
                Opacity = 0.5;
            }
            else
            {
                Width = 320;
                ChangeBarsSizes(Width);
                Opacity = 1;
            }
        }
        #endregion

        #region Settings
        public void ApplySettings()
        {
            UserSettings.Config.Monsterscomponent config = UserSettings.PlayerConfig.Overlay.MonstersComponent;
            MonsterPartsContainer.Visibility = !config.EnableMonsterParts && !config.EnableRemovableParts ? Visibility.Collapsed : Visibility.Visible;
            MonsterAilmentsContainer.Visibility = !config.EnableMonsterAilments ? Visibility.Collapsed : Visibility.Visible;
            foreach (Monster_Widget.Parts.MonsterPart part in MonsterPartsContainer.Children)
            {
                part.ApplySettings();
            }
            foreach (Monster_Widget.Parts.MonsterAilment ailment in MonsterAilmentsContainer.Children)
            {
                ailment.ApplySettings();
            }
            if (Context != null)
            {
                SetMonsterHealthBarText(Context.CurrentHP, Context.TotalHP);
            }
        }
        #endregion

        #region Helpers
        private void ChangeBarsSizes(double NewSize)
        {
            UserSettings.Config.Monsterscomponent config = UserSettings.PlayerConfig.Overlay.MonstersComponent;
            // Parts
            MonsterPartsContainer.MaxWidth = config.EnableMonsterAilments ? (NewSize - 2) / 2 : (NewSize - 1);
            MonsterAilmentsContainer.MaxWidth = config.EnableMonsterParts ? (NewSize - 2) / 2 : (NewSize - 1);
            MonsterPartsContainer.ItemWidth = MonsterAilmentsContainer.ItemWidth = MonsterPartsContainer.MaxWidth / Math.Max(1, UserSettings.PlayerConfig.Overlay.MonstersComponent.MaxPartColumns);
            UpdateContainerBarsSizeDynamically();
            // Monster Bar
            MonsterHealthBar.MaxSize = NewSize - 69;
            MonsterStaminaBar.MaxSize = NewSize - 72;
            MonsterHealthBar.UpdateBar(Context.CurrentHP, Context.TotalHP);
            MonsterStaminaBar.UpdateBar(Context.Stamina, Context.MaxStamina);
        }

        private void UpdateContainerBarsSizeDynamically()
        {
            UserSettings.Config.Monsterscomponent config = UserSettings.PlayerConfig.Overlay.MonstersComponent;
            int numberOfPartsDisplayed = MonsterPartsContainer.Children.Cast<Monster_Widget.Parts.MonsterPart>()
                .Where(p => p.IsVisible)
                .Count();

            int numberOfAilmentsDisplayed = MonsterAilmentsContainer.Children.Cast<Monster_Widget.Parts.MonsterAilment>()
                .Where(a => a.IsVisible)
                .Count();
            
            MonsterPartsContainer.ItemWidth = MonsterPartsContainer.MaxWidth / Math.Min(config.MaxPartColumns, Math.Max(1, Math.Ceiling(numberOfPartsDisplayed / (double)config.MaxNumberOfPartsAtOnce)));
            MonsterAilmentsContainer.ItemWidth = MonsterAilmentsContainer.MaxWidth / Math.Min(config.MaxPartColumns, Math.Max(1, Math.Ceiling(numberOfAilmentsDisplayed / (double)config.MaxNumberOfPartsAtOnce)));

            foreach (Monster_Widget.Parts.MonsterPart part in MonsterPartsContainer.Children)
            {
                part.UpdateHealthBarSize(MonsterPartsContainer.ItemWidth);
            }

            foreach (Monster_Widget.Parts.MonsterAilment ailment in MonsterAilmentsContainer.Children)
            {
                ailment.UpdateBarSize(MonsterAilmentsContainer.ItemWidth);
            }
            

        }

        public void ChangeDocking(byte newDock)
        {

            switch (newDock)
            {
                case 0: // Monster HP stays on top
                    DockPanel.SetDock(MonsterHealthContainer, Dock.Top);
                    break;
                case 1:
                    DockPanel.SetDock(MonsterHealthContainer, Dock.Bottom);
                    break;
            }
        }

        private void SetMonsterHealthBarText(float Health, float TotalHealth)
        {
            string HealthStringFormat = UserSettings.PlayerConfig.Overlay.MonstersComponent.HealthTextFormat;
            HealthStringFormat = HealthStringFormat.Replace("{Health:0}", Health.ToString("0"))
                .Replace("{Health:0.0}", Health.ToString("0.0"))
                .Replace("{TotalHealth:0}", TotalHealth.ToString("0"))
                .Replace("{TotalHealth:0.0}", TotalHealth.ToString("0.0"))
                .Replace("{Percentage:0}", (Health / TotalHealth * 100).ToString("0"))
                .Replace("{Percentage:0.0}", (Health / TotalHealth * 100).ToString("0.0"));
            HealthText.Text = HealthStringFormat;
        }

        private void SetMonsterStaminaText(float stam, float max_stam) => StaminaText.Text = $"{stam:0}/{max_stam:0}";

        private BitmapImage GetMonsterIcon(string MonsterEm)
        {
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"HunterPie.Resources\Monsters\Icons\{MonsterEm}.png"))) return null;
            Uri ImageURI = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"HunterPie.Resources\Monsters\Icons\{MonsterEm}.png"), UriKind.Absolute);
            BitmapImage mIcon = new BitmapImage(ImageURI);
            mIcon.Freeze();
            return mIcon;
        }
        #endregion

        private void OnMonsterPartsContainerSizeChange(object sender, SizeChangedEventArgs e) => UpdateContainerBarsSizeDynamically();
        private void OnMonsterAilmentsContainerSizeChange(object sender, SizeChangedEventArgs e) => UpdateContainerBarsSizeDynamically();


        
    }
}
