using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HunterPie.Core;
using HunterPie.Core.Enums;
using HunterPie.Core.Events;
using HunterPie.GUI.Widgets.Monster_Widget.Parts;
using HunterPie.GUIControls.Custom_Controls;

using AlatreonState = HunterPie.Core.Enums.AlatreonState;
using BitmapImage = System.Windows.Media.Imaging.BitmapImage;
using Timer = System.Threading.Timer;

namespace HunterPie.GUI.Widgets
{
    /// <summary>
    /// Interaction logic for MonsterHealth.xaml
    /// </summary>
    public partial class MonsterHealth : UserControl
    {

        public string ActionName
        {
            get { return (string)GetValue(ActionNameProperty); }
            set { SetValue(ActionNameProperty, value); }
        }
        public static readonly DependencyProperty ActionNameProperty =
            DependencyProperty.Register("ActionName", typeof(string), typeof(MonsterHealth));

        public Visibility ActionVisibility
        {
            get { return (Visibility)GetValue(ActionVisibilityProperty); }
            set { SetValue(ActionVisibilityProperty, value); }
        }
        public static readonly DependencyProperty ActionVisibilityProperty =
            DependencyProperty.Register("ActionVisibility", typeof(Visibility), typeof(MonsterHealth));

        private Monster Context;
        private Timer VisibilityTimer;

        // Animations
        private Storyboard ANIM_ENRAGEDICON;

        public MonsterHealth() => InitializeComponent();
        public int NumberOfPartsDisplayed = 0;
        public int NumberOfAilmentsDisplayed = 0;

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
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode == 3)
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
            Context.OnMonsterCapture += OnMonsterDespawn;
            Context.OnHPUpdate += OnMonsterUpdate;
            Context.OnStaminaUpdate += OnStaminaUpdate;
            Context.OnEnrage += OnEnrage;
            Context.OnUnenrage += OnUnenrage;
            Context.OnEnrageTimerUpdate += OnEnrageTimerUpdate;
            Context.OnTargetted += OnMonsterTargetted;
            Context.OnCrownChange += OnMonsterCrownChange;
            Context.OnAlatreonElementShift += OnAlatreonElementShift;
            Context.OnMonsterAilmentsCreate += OnMonsterAilmentsCreate;
            Context.OnActionChange += OnActionChange;
        }

        public void UnhookEvents()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                foreach (MonsterPart Part in MonsterPartsContainer.Children)
                {
                    Part.UnhookEvents();
                }
                foreach (MonsterAilment Ailment in MonsterAilmentsContainer.Children)
                {
                    Ailment.UnhookEvents();
                }
                MonsterAilmentsContainer.Children.Clear();
                MonsterPartsContainer.Children.Clear();
            }));
            Context.OnMonsterSpawn -= OnMonsterSpawn;
            Context.OnMonsterDespawn -= OnMonsterDespawn;
            Context.OnMonsterDeath -= OnMonsterDespawn;
            Context.OnMonsterCapture -= OnMonsterDespawn;
            Context.OnHPUpdate -= OnMonsterUpdate;
            Context.OnStaminaUpdate -= OnStaminaUpdate;
            Context.OnEnrage -= OnEnrage;
            Context.OnUnenrage -= OnUnenrage;
            Context.OnEnrageTimerUpdate -= OnEnrageTimerUpdate;
            Context.OnTargetted -= OnMonsterTargetted;
            Context.OnCrownChange -= OnMonsterCrownChange;
            Context.OnAlatreonElementShift -= OnAlatreonElementShift;
            Context.OnMonsterAilmentsCreate -= OnMonsterAilmentsCreate;
            Context.OnActionChange -= OnActionChange;
            Context = null;
        }

        #region Monster Events
        private void UpdateMonsterInfo(Monster Monster)
        {
            Visibility = Visibility.Visible;
            MonsterName.Text = Monster.Name;
            MonsterHealthBar.MaxSize = Width * 0.7833333333333333;
            MonsterStaminaBar.MaxSize = Width - 72;

            // Update monster health and stamina
            UpdateHealthBar(MonsterHealthBar, Monster.Health, Monster.MaxHealth);
            UpdateHealthBar(MonsterStaminaBar, Monster.Stamina, Monster.MaxStamina);
            SetMonsterHealthBarText(Monster.Health, Monster.MaxHealth);
            SetMonsterStaminaText(Monster.Stamina, Monster.MaxStamina);
            DisplayCapturableIcon(Monster.Health, Monster.MaxHealth, Monster.CaptureThreshold);

            // Gets monster icon
            MonsterIcon.Source = GetMonsterIcon(Monster.Id);

            SwitchSizeBasedOnTarget();

            // Parts
            int index = 0;
            MonsterPartsContainer.Children.Clear();
            while (index < Monster.Parts.Count)
            {
                Part mPart = Monster.Parts[index];
                MonsterPart PartDisplay = new MonsterPart()
                {
                    Style = FindResource("OVERLAY_MONSTER_SUB_PART_STYLE") as Style
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
                MonsterAilment AilmentDisplay = new MonsterAilment()
                {
                    Style = FindResource("OVERLAY_MONSTER_SUB_AILMENT_STYLE") as Style
                };
                AilmentDisplay.SetContext(ailment, MonsterAilmentsContainer.ItemWidth);
                MonsterAilmentsContainer.Children.Add(AilmentDisplay);
                index++;
            }

            // Enrage
            if (Monster.IsEnraged && !UserSettings.PlayerConfig.Overlay.MonstersComponent.HideHealthInformation)
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
            index = 0;
            while (index < Monster.Weaknesses.Keys.Count)
            {
                string Weakness = Monster.Weaknesses.Keys.ElementAt(index);
                ImageSource img = FindResource(Weakness) as ImageSource;
                img?.Freeze();
                WeaknessDisplay MonsterWeaknessDisplay = new WeaknessDisplay
                {
                    Icon = img,
                    Width = 20,
                    Height = 20
                };
                Weaknesses.Children.Add(MonsterWeaknessDisplay);
                index++;
            }
            // Sometimes Alatreon's state changes before OnMonsterSpawn is dispatched
            if (Monster.GameId == 87) OnAlatreonElementShift(this, EventArgs.Empty);
        }


        private void OnActionChange(object source, MonsterUpdateEventArgs args)
        {
            Dispatch(() =>
            {
                ActionName = args.Action;
            });
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
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.HideHealthInformation) return;
            ANIM_ENRAGEDICON.Begin(MonsterHealthBar, true);
            ANIM_ENRAGEDICON.Begin(HealthBossIcon, true);
        });

        private void OnEnrageTimerUpdate(object source, MonsterUpdateEventArgs args)
        {
            if (Context == null || UserSettings.PlayerConfig.Overlay.MonstersComponent.HideHealthInformation) return;
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
            UpdateHealthBar(MonsterStaminaBar, args.Stamina, args.MaxStamina);
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
            foreach (MonsterPart Part in MonsterPartsContainer.Children)
            {
                Part.UnhookEvents();
            }
            foreach (MonsterAilment Ailment in MonsterAilmentsContainer.Children)
            {
                Ailment.UnhookEvents();
            }
            MonsterAilmentsContainer.Children.Clear();
            MonsterPartsContainer.Children.Clear();
        });

        private void OnMonsterSpawn(object source, MonsterSpawnEventArgs args) => Dispatch(() =>
        {
            UpdateMonsterInfo(Context);
        });

        private void OnMonsterUpdate(object source, MonsterUpdateEventArgs args) => Dispatch(() =>
        {
            UpdateHealthBar(MonsterHealthBar, args.Health, args.MaxHealth);
            SetMonsterHealthBarText(args.Health, args.MaxHealth);
            DisplayCapturableIcon(args.Health, args.MaxHealth, Context.CaptureThreshold);
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode == 3)
            {
                Visibility = Visibility.Visible;
                StartVisibilityTimer();
            }
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.EnableSortParts)
            {
                SortParts();
            }
        });

        private void OnAlatreonElementShift(object source, EventArgs args) => Dispatch(() =>
        {
            Weaknesses.Children.Clear();
            string[] newWeaknesses;
            switch(Context?.AlatreonElement)
            {
                case AlatreonState.Fire:
                    newWeaknesses = new string[2] { "ELEMENT_ICE", "ELEMENT_WATER" };
                    break;
                case AlatreonState.Ice:
                    newWeaknesses = new string[2] { "ELEMENT_FIRE", "ELEMENT_THUNDER" };
                    break;
                case AlatreonState.Dragon:
                    newWeaknesses = new string[3] { "ELEMENT_DRAGON", "ELEMENT_ICE", "ELEMENT_FIRE" };
                    break;
                default:
                    return;
            }
            foreach (string weaknessId in newWeaknesses)
            {
                ImageSource img = FindResource(weaknessId) as ImageSource;
                img?.Freeze();
                WeaknessDisplay weaknessDisplay = new WeaknessDisplay
                {
                    Icon = img,
                    Width = 20,
                    Height = 20
                };
                Weaknesses.Children.Add(weaknessDisplay);
            };
        });

        private void OnMonsterAilmentsCreate(object source, EventArgs args)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Render, new Action(() =>
            {
                // Ailments
                int index = 0;
                MonsterAilmentsContainer.Children.Clear();
                while (index < Context.Ailments.Count)
                {
                    Ailment ailment = Context.Ailments[index];
                    MonsterAilment AilmentDisplay = new Monster_Widget.Parts.MonsterAilment()
                    {
                        Style = FindResource("OVERLAY_MONSTER_SUB_AILMENT_STYLE") as Style
                    };
                    AilmentDisplay.SetContext(ailment, MonsterAilmentsContainer.ItemWidth);
                    MonsterAilmentsContainer.Children.Add(AilmentDisplay);
                    index++;
                }
            }));
        }

        #endregion


        #region Visibility timer
        private void StartVisibilityTimer()
        {
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode != 3)
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
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode != 3)
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
            switch ((MonsterBarMode)UserSettings.PlayerConfig.Overlay.MonstersComponent.ShowMonsterBarMode)
            {
                case MonsterBarMode.Default:
                    ShowAllMonstersAtOnce();
                    break;
                case MonsterBarMode.ShowAllButFocusTarget:
                    ShowAllButFocusTarget();
                    break;
                case MonsterBarMode.ShowOnlyTarget:
                    ShowOnlyTargetMonster();
                    break;
                case MonsterBarMode.ShowAllButHideInactive:
                    ShowAllMonsterAndHideUnactive();
                    break;
                case MonsterBarMode.ShowAllOrSelected:
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


        // Show all monsters but hide inactive
        // FIXME: typo
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
            ActionVisibility = config.ShowMonsterActionName ? Visibility.Visible : Visibility.Collapsed;
            foreach (MonsterPart part in MonsterPartsContainer.Children)
            {
                part.ApplySettings();
            }
            foreach (MonsterAilment ailment in MonsterAilmentsContainer.Children)
            {
                ailment.ApplySettings();
            }
            if (Context != null)
            {
                SetMonsterHealthBarText(Context.Health, Context.MaxHealth);
            }
        }
        #endregion

        #region Helpers

        private void SortParts()
        {
            List<MonsterPart> parts = new List<MonsterPart>();
            foreach (MonsterPart part in MonsterPartsContainer.Children)
            {
                parts.Add(part);
            }
            parts.Sort((l, r) => r.CompareTo(l));
            MonsterPartsContainer.Children.Clear();
            parts.ForEach(part => MonsterPartsContainer.Children.Add(part));
        }

        public void ChangeBarsSizes(double NewSize)
        {

            UserSettings.Config.Monsterscomponent config = UserSettings.PlayerConfig.Overlay.MonstersComponent;
            // Parts
            MonsterPartsContainer.MaxWidth = config.EnableMonsterAilments ? (NewSize - 2) / 2 : (NewSize - 1);
            MonsterAilmentsContainer.MaxWidth = config.EnableMonsterParts || config.EnableRemovableParts ? (NewSize - 2) / 2 : (NewSize - 1);
            UpdateContainerBarsSizeDynamically();
            MonsterHealthBar.MaxSize = NewSize - 69;
            MonsterStaminaBar.MaxSize = NewSize - 72;
            UpdateHealthBar(MonsterHealthBar, Context.Health, Context.MaxHealth);
            UpdateHealthBar(MonsterStaminaBar, Context.Stamina, Context.MaxStamina);
        }

        private void UpdateContainerBarsSizeDynamically()
        {
            UserSettings.Config.Monsterscomponent config = UserSettings.PlayerConfig.Overlay.MonstersComponent;
            NumberOfPartsDisplayed = MonsterPartsContainer.Children.Cast<Monster_Widget.Parts.MonsterPart>()
                .Where(p => p.IsVisible)
                .Count();

            NumberOfAilmentsDisplayed = MonsterAilmentsContainer.Children.Cast<Monster_Widget.Parts.MonsterAilment>()
                .Where(a => a.IsVisible)
                .Count();

            double partsContainerWidth = MonsterPartsContainer.MaxWidth / Math.Max(1, Math.Min(config.MaxPartColumns, Math.Max(1, Math.Ceiling(NumberOfPartsDisplayed / (double)config.MaxNumberOfPartsAtOnce))));
            double ailmentsContainerWidth = MonsterAilmentsContainer.MaxWidth / Math.Max(1, Math.Min(config.MaxPartColumns, Math.Max(1, Math.Ceiling(NumberOfAilmentsDisplayed / (double)config.MaxNumberOfPartsAtOnce))));

            MonsterPartsContainer.ItemWidth = double.IsInfinity(partsContainerWidth) ? (MonsterPartsContainer.Width - 2) / 2 : partsContainerWidth;
            MonsterAilmentsContainer.ItemWidth = double.IsInfinity(ailmentsContainerWidth) ? (MonsterPartsContainer.Width - 2) / 2 : ailmentsContainerWidth;

            foreach (Monster_Widget.Parts.MonsterPart part in MonsterPartsContainer.Children)
            {
                part.UpdateHealthSize(MonsterPartsContainer.ItemWidth);
            }

            foreach (Monster_Widget.Parts.MonsterAilment ailment in MonsterAilmentsContainer.Children)
            {
                ailment.UpdateSize(MonsterAilmentsContainer.ItemWidth);
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
                    // TODO: Fix bottom dock
                    DockPanel.SetDock(MonsterHealthContainer, Dock.Top);
                    break;
            }
        }

        private void SetMonsterHealthBarText(float Health, float TotalHealth)
        {
            if (UserSettings.PlayerConfig.Overlay.MonstersComponent.HideHealthInformation)
            {
                HealthText.Text = string.Empty;
            }
            else
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

        }

        private void SetMonsterStaminaText(float stam, float maxStam)
        {
            StaminaText.Text = UserSettings.PlayerConfig.Overlay.MonstersComponent.HideHealthInformation ? string.Empty : $"{stam:0}/{maxStam:0}";
        }

        private BitmapImage GetMonsterIcon(string MonsterEm)
        {
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"HunterPie.Resources\Monsters\Icons\{MonsterEm}.png"))) return null;
            Uri ImageURI = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"HunterPie.Resources\Monsters\Icons\{MonsterEm}.png"), UriKind.Absolute);
            BitmapImage mIcon = new BitmapImage(ImageURI);
            mIcon.Freeze();
            return mIcon;
        }

        private void DisplayCapturableIcon(float monsterHealth, float monsterMaxHealth, float monsterCaptureThreshold)
        {
            bool captureable = (monsterHealth / monsterMaxHealth * 100) < monsterCaptureThreshold;
            if (!UserSettings.PlayerConfig.Overlay.MonstersComponent.HideHealthInformation && captureable)
            {
                CapturableIcon.Visibility = Visibility.Visible;
            }
            else
            {
                CapturableIcon.Visibility = Visibility.Hidden;
            }

        }

        private void UpdateHealthBar(MinimalHealthBar healthBar, float newValue, float maxValue)
        {
            float updateValue = UserSettings.PlayerConfig.Overlay.MonstersComponent.HideHealthInformation
                ? maxValue
                : newValue;
            healthBar.UpdateBar(updateValue, maxValue);
        }
        #endregion

        private void OnMonsterPartsContainerSizeChange(object sender, SizeChangedEventArgs e) => UpdateContainerBarsSizeDynamically();
        private void OnMonsterAilmentsContainerSizeChange(object sender, SizeChangedEventArgs e) => UpdateContainerBarsSizeDynamically();

    }
}
