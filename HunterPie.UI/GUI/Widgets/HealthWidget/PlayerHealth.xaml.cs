using System;
using System.Windows;
using System.Windows.Threading;
using HunterPie.Core;
using HunterPie.Core.Events;
using HunterPie.Core.Enums;
using System.Windows.Media;
using HunterPie.GUI.Widgets.HealthWidget.Parts;
using HunterPie.Core.Local;
using System.Collections.Generic;
using HunterPie.GUIControls.Custom_Controls;
using System.Windows.Shapes;
using System.Windows.Controls;
using HunterPie.GUI.Helpers;
using System.Linq;
using HunterPie.Core.Settings;
using HunterPie.Core.Native;

namespace HunterPie.GUI.Widgets.HealthWidget
{
    /// <summary>
    /// Interaction logic for PlayerHealth.xaml
    /// </summary>
    public partial class PlayerHealth : Widget
    {
        public override WidgetType Type => WidgetType.PlayerWidget;

        private Game gContext { get; set; }
        private Player Context => gContext.Player;

        public override IWidgetSettings Settings => ConfigManager.Settings.Overlay.PlayerHealthComponent;
        private PlayerHealthComponent settings => (PlayerHealthComponent)Settings;

        private const double SharpnessMaxWidth = 50;

        public string PlayerName
        {
            get { return (string)GetValue(PlayerNameProperty); }
            set { SetValue(PlayerNameProperty, value); }
        }
        public static readonly DependencyProperty PlayerNameProperty =
            DependencyProperty.Register("PlayerName", typeof(string), typeof(PlayerHealth));

        public string DayTimeIcon
        {
            get { return (string)GetValue(DayTimeIconProperty); }
            set { SetValue(DayTimeIconProperty, value); }
        }
        public static readonly DependencyProperty DayTimeIconProperty =
            DependencyProperty.Register("DayTimeIcon", typeof(string), typeof(PlayerHealth));

        public string PlayerLaurel
        {
            get { return (string)GetValue(PlayerLaurelProperty); }
            set { SetValue(PlayerLaurelProperty, value); }
        }
        public static readonly DependencyProperty PlayerLaurelProperty =
            DependencyProperty.Register("PlayerLaurel", typeof(string), typeof(PlayerHealth));

        public Brush SharpnessLowerColor
        {
            get { return (Brush)GetValue(SharpnessLowerColorProperty); }
            set { SetValue(SharpnessLowerColorProperty, value); }
        }
        public static readonly DependencyProperty SharpnessLowerColorProperty =
            DependencyProperty.Register("SharpnessLowerColor", typeof(Brush), typeof(PlayerHealth));

        public Brush SharpnessColor
        {
            get { return (Brush)GetValue(SharpnessColorProperty); }
            set { SetValue(SharpnessColorProperty, value); }
        }
        public static readonly DependencyProperty SharpnessColorProperty =
            DependencyProperty.Register("SharpnessColor", typeof(Brush), typeof(PlayerHealth));

        public double Sharpness
        {
            get { return (double)GetValue(SharpnessProperty); }
            set { SetValue(SharpnessProperty, value); }
        }
        public static readonly DependencyProperty SharpnessProperty =
            DependencyProperty.Register("Sharpness", typeof(double), typeof(PlayerHealth));

        public Visibility SharpnessVisibility
        {
            get { return (Visibility)GetValue(SharpnessVisibilityProperty); }
            set { SetValue(SharpnessVisibilityProperty, value); }
        }
        public static readonly DependencyProperty SharpnessVisibilityProperty =
            DependencyProperty.Register("SharpnessVisibility", typeof(Visibility), typeof(PlayerHealth));

        public bool IsStaminaNormal
        {
            get { return (bool)GetValue(IsStaminaNormalProperty); }
            set { SetValue(IsStaminaNormalProperty, value); }
        }
        public static readonly DependencyProperty IsStaminaNormalProperty =
            DependencyProperty.Register("IsStaminaNormal", typeof(bool), typeof(PlayerHealth));

        public bool IsWet
        {
            get { return (bool)GetValue(IsWetProperty); }
            set { SetValue(IsWetProperty, value); }
        }
        public static readonly DependencyProperty IsWetProperty =
            DependencyProperty.Register("IsWet", typeof(bool), typeof(PlayerHealth));

        public bool IsIcy
        {
            get { return (bool)GetValue(IsIcyProperty); }
            set { SetValue(IsIcyProperty, value); }
        }
        public static readonly DependencyProperty IsIcyProperty =
            DependencyProperty.Register("IsIcy", typeof(bool), typeof(PlayerHealth));

        public int PlayerCurrentStamina
        {
            get { return (int)GetValue(PlayerCurrentStaminaProperty); }
            set { SetValue(PlayerCurrentStaminaProperty, value); }
        }
        public static readonly DependencyProperty PlayerCurrentStaminaProperty =
            DependencyProperty.Register("PlayerCurrentStamina", typeof(int), typeof(PlayerHealth));

        public int PlayerMaxStamina
        {
            get { return (int)GetValue(PlayerMaxStaminaProperty); }
            set { SetValue(PlayerMaxStaminaProperty, value); }
        }
        public static readonly DependencyProperty PlayerMaxStaminaProperty =
            DependencyProperty.Register("PlayerMaxStamina", typeof(int), typeof(PlayerHealth));

        public int PlayerCurrentHealth
        {
            get { return (int)GetValue(PlayerCurrentHealthProperty); }
            set { SetValue(PlayerCurrentHealthProperty, value); }
        }
        public static readonly DependencyProperty PlayerCurrentHealthProperty =
            DependencyProperty.Register("PlayerCurrentHealth", typeof(int), typeof(PlayerHealth));

        public int PlayerMaxHealth
        {
            get { return (int)GetValue(PlayerMaxHealthProperty); }
            set { SetValue(PlayerMaxHealthProperty, value); }
        }
        public static readonly DependencyProperty PlayerMaxHealthProperty =
            DependencyProperty.Register("PlayerMaxHealth", typeof(int), typeof(PlayerHealth));

        public int PlayerCurrentSharpness
        {
            get { return (int)GetValue(PlayerCurrentSharpnessProperty); }
            set { SetValue(PlayerCurrentSharpnessProperty, value); }
        }
        public static readonly DependencyProperty PlayerCurrentSharpnessProperty =
            DependencyProperty.Register("PlayerCurrentSharpness", typeof(int), typeof(PlayerHealth));

        public int PlayerMaxSharpness
        {
            get { return (int)GetValue(PlayerMaxSharpnessProperty); }
            set { SetValue(PlayerMaxSharpnessProperty, value); }
        }
        public static readonly DependencyProperty PlayerMaxSharpnessProperty =
            DependencyProperty.Register("PlayerMaxSharpness", typeof(int), typeof(PlayerHealth));

        public int PlayerMinSharpness
        {
            get { return (int)GetValue(PlayerMinSharpnessProperty); }
            set { SetValue(PlayerMinSharpnessProperty, value); }
        }
        public static readonly DependencyProperty PlayerMinSharpnessProperty =
            DependencyProperty.Register("PlayerMinSharpness", typeof(int), typeof(PlayerHealth));

        public int PlayerSharpnessLeft
        {
            get { return (int)GetValue(PlayerSharpnessLeftProperty); }
            set { SetValue(PlayerSharpnessLeftProperty, value); }
        }
        public static readonly DependencyProperty PlayerSharpnessLeftProperty =
            DependencyProperty.Register("PlayerSharpnessLeft", typeof(int), typeof(PlayerHealth));

        public Visibility MoxieVisibility
        {
            get { return (Visibility)GetValue(MoxieVisibilityProperty); }
            set { SetValue(MoxieVisibilityProperty, value); }
        }
        public static readonly DependencyProperty MoxieVisibilityProperty =
            DependencyProperty.Register("MoxieVisibility", typeof(Visibility), typeof(PlayerHealth));

        public Thickness MoxieMargin
        {
            get { return (Thickness)GetValue(MoxieMarginProperty); }
            set { SetValue(MoxieMarginProperty, value); }
        }
        public static readonly DependencyProperty MoxieMarginProperty =
            DependencyProperty.Register("MoxieMargin", typeof(Thickness), typeof(PlayerHealth));


        public static readonly DependencyProperty IsMinimalisticModeProperty = DependencyProperty.Register(
            "IsMinimalisticMode", typeof(bool), typeof(PlayerHealth), new PropertyMetadata(default(bool)));

        public bool IsMinimalisticMode
        {
            get { return (bool)GetValue(IsMinimalisticModeProperty); }
            set { SetValue(IsMinimalisticModeProperty, value); }
        }

        public bool HasHotDrink
        {
            get { return (bool)GetValue(HasHotDrinkProperty); }
            set { SetValue(HasHotDrinkProperty, value); }
        }

        public static readonly DependencyProperty HasHotDrinkProperty =
            DependencyProperty.Register("HasHotDrink", typeof(bool), typeof(PlayerHealth));

        MinimalHealthBar StaminaBar { get; set; }
        HealthBar HealthBar { get; set; }
        Rectangle HealthExt { get; set; }
        Rectangle StaminaExt { get; set; }

        Panel HealthBarPanel { get; set; }
        AilmentControl ConstantAilment { get; set; }

        public PlayerHealth(Game ctx)
        {
            IsStaminaNormal = true;
            SharpnessVisibility = Visibility.Collapsed;
            IsMinimalisticMode = ConfigManager.Settings.Overlay.PlayerHealthComponent.MinimalisticMode;
            gContext = ctx;
            InitializeComponent();

        }

        public override void EnterWidgetDesignMode()
        {
            base.EnterWidgetDesignMode();
            RemoveWindowTransparencyFlag();
        }

        public override void LeaveWidgetDesignMode()
        {
            ApplyWindowTransparencyFlag();
            base.LeaveWidgetDesignMode();
        }

        public override void ApplySettings()
        {
            PlayerName = FormatNameString();

            if ((Context?.ZoneID ?? 0) == 0)
            {
                WidgetHasContent = false;
            }
            else
                WidgetHasContent = settings.HideHealthInVillages ? !(Context?.InHarvestZone ?? true) : true;

            base.ApplySettings();
        }

        private void UpdateInformation()
        {
            OnMaxHealthUpdate(this, new PlayerHealthEventArgs(Context.Health));
            OnZoneChange(this, new PlayerLocationEventArgs(Context));
            OnMaxStaminaUpdate(this, new PlayerStaminaEventArgs(Context.Stamina));
            OnStaminaUpdate(this, new PlayerStaminaEventArgs(Context.Stamina));
            OnLevelChange(this, new PlayerEventArgs(Context));
            OnWorldDayTimeUpdate(this, new WorldEventArgs(gContext));
            if (Context.CurrentWeapon != null)
            {
                SharpnessVisibility = Context.CurrentWeapon.IsMelee ? Visibility.Visible : Visibility.Collapsed;
                OnSharpnessLevelChange(this, new SharpnessEventArgs(Context.CurrentWeapon));
                OnSharpnessChange(this, new SharpnessEventArgs(Context.CurrentWeapon));
            }
        }

        private void HookEvents()
        {
            gContext.OnWorldDayTimeUpdate += OnWorldDayTimeUpdate;
            Context.OnZoneChange += OnZoneChange;
            Context.Health.OnHealthUpdate += OnHealthUpdate;
            Context.Health.OnMaxHealthUpdate += OnMaxHealthUpdate;
            Context.Health.OnHealHealth += OnHealHealth;
            Context.Health.OnRedHealthUpdate += OnRedHealthUpdate;
            Context.Health.OnHealthExtStateUpdate += OnHealthExtStateUpdate;
            Context.Stamina.OnStaminaUpdate += OnStaminaUpdate;
            Context.Stamina.OnMaxStaminaUpdate += OnMaxStaminaUpdate;
            Context.Stamina.OnStaminaExtStateUpdate += OnStaminaExtStateUpdate;
            Context.OnAilmentUpdate += OnAilmentUpdate;
            Context.OnLevelChange += OnLevelChange;
            Context.OnHotDrinkStateChange += OnHotDrinkStateChange;

            if (Context.CurrentWeapon != null)
            {
                Context.CurrentWeapon.OnSharpnessChange += OnSharpnessChange;
                Context.CurrentWeapon.OnSharpnessLevelChange += OnSharpnessLevelChange;
            }

            Context.OnClassChange += OnClassChange;

            // To track debuffs
            Context.Abnormalities.OnNewAbnormality += OnNewAbnormality;
            Context.Abnormalities.OnAbnormalityRemove += OnAbnormalityEnd;
        }

        public void UnhookEvents()
        {
            gContext.OnWorldDayTimeUpdate -= OnWorldDayTimeUpdate;
            Context.OnZoneChange -= OnZoneChange;
            Context.Health.OnHealthUpdate -= OnHealthUpdate;
            Context.Health.OnMaxHealthUpdate -= OnMaxHealthUpdate;
            Context.Health.OnHealHealth -= OnHealHealth;
            Context.Health.OnRedHealthUpdate -= OnRedHealthUpdate;
            Context.Health.OnHealthExtStateUpdate -= OnHealthExtStateUpdate;
            Context.Stamina.OnStaminaUpdate -= OnStaminaUpdate;
            Context.Stamina.OnMaxStaminaUpdate -= OnMaxStaminaUpdate;
            Context.Stamina.OnStaminaExtStateUpdate -= OnStaminaExtStateUpdate;
            Context.OnAilmentUpdate -= OnAilmentUpdate;
            Context.OnLevelChange -= OnLevelChange;
            Context.OnHotDrinkStateChange -= OnHotDrinkStateChange;

            if (Context.CurrentWeapon != null)
            {
                Context.CurrentWeapon.OnSharpnessChange -= OnSharpnessChange;
                Context.CurrentWeapon.OnSharpnessLevelChange -= OnSharpnessLevelChange;
            }

            Context.OnClassChange -= OnClassChange;

            Context.Abnormalities.OnNewAbnormality -= OnNewAbnormality;
            Context.Abnormalities.OnAbnormalityRemove -= OnAbnormalityEnd;
        }


        private void OnHotDrinkStateChange(object source, EventArgs args)
        {
            Dispatcher.InvokeAsync(async () =>
            {
                PlayerEventArgs e = (PlayerEventArgs)args;
                if (!e.HasHotDrink)
                    await Chat.SystemMessage("<STYL MOJI_RED_SELECTED><ICON SLG_NEWS> Warning:</STYL>\nMissing Hot Drink!", 0, 0, 0);

                HasHotDrink = e.HasHotDrink;
            });

        }

        private void OnZoneChange(object source, EventArgs args)
        {
            PlayerLocationEventArgs e = args as PlayerLocationEventArgs;
            Dispatcher.InvokeAsync(() =>
            {
                if (e.ZoneId != 0)
                {
                    if (settings.HideHealthInVillages)
                    {
                        WidgetHasContent = !e.InHarvestZone;
                    } else
                    {
                        WidgetHasContent = true;
                    }
                } else
                {
                    WidgetHasContent = false;
                }
                ChangeVisibility();
            });
        }

        private void OnAbnormalityEnd(object source, AbnormalityEventArgs args)
        {
            HashSet<string> poison = new HashSet<string>()
            {
                "ICON_POISON", "ICON_VENOM"
            };
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                string abnormIcon = args.Abnormality.Icon;

                if (args.Abnormality.IsDebuff)
                {
                    if (abnormIcon == "ELEMENT_FIRE") HealthBar.IsOnFire = false;
                    else if (abnormIcon == "ICON_BLEED") HealthBar.IsBleeding = false;
                    else if (abnormIcon == "ICON_EFFLUVIA") HealthBar.HasEffluvia = false;
                    else if (abnormIcon == "ELEMENT_WATER") IsWet = false;
                    else if (abnormIcon == "ELEMENT_ICE") IsIcy = false;
                    else if (poison.Contains(abnormIcon)) HealthBar.IsPoisoned = false;

                    HealthBar.IsNormal = (!HealthBar.IsOnFire && !HealthBar.IsPoisoned && !HealthBar.IsBleeding && !HealthBar.HasEffluvia);
                    IsStaminaNormal = (!IsWet && !IsIcy);
                } else
                {
                    if (abnormIcon == "ICON_NATURALHEALING") HealthBar.IsHealing = false;
                }
            }));
        }

        private void OnNewAbnormality(object source, AbnormalityEventArgs args)
        {
            HashSet<string> poison = new HashSet<string>()
            {
                "ICON_POISON", "ICON_VENOM"
            };
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                string abnormIcon = args.Abnormality.Icon;

                if (args.Abnormality.IsDebuff)
                {

                    if (abnormIcon == "ELEMENT_FIRE") HealthBar.IsOnFire = true;
                    else if (abnormIcon == "ICON_BLEED") HealthBar.IsBleeding = true;
                    else if (abnormIcon == "ICON_EFFLUVIA") HealthBar.HasEffluvia = true;
                    else if (abnormIcon == "ELEMENT_WATER") IsWet = true;
                    else if (abnormIcon == "ELEMENT_ICE") IsIcy = true;
                    else if (poison.Contains(abnormIcon)) HealthBar.IsPoisoned = true;

                } else
                {
                    if (abnormIcon == "ICON_NATURALHEALING") HealthBar.IsHealing = true;
                }
            }));
        }

        private void OnHealthExtStateUpdate(object source, PlayerHealthEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                if (args.IsHealthExtVisible && args.MaxPossibleHealth != args.MaxHealth)
                {
                    HealthExt.Visibility = Visibility.Visible;

                    float maxHealth = Math.Min(args.MaxHealth + HealthComponent.CanIncreaseMaxHealth[args.SelectedItemId], args.MaxPossibleHealth);
                    double extWidth = HealthBar.ConstantWidth * ((maxHealth - args.MaxHealth) / HealthBar.CHealth);
                    HealthExt.Width = Math.Max(0, extWidth);
                } else
                {
                    HealthExt.Width = 0;
                    HealthExt.Visibility = Visibility.Collapsed;
                }
            }));
        }

        private void OnStaminaExtStateUpdate(object source, PlayerStaminaEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                if (args.IsStaminaExtVisible && args.MaxPossibleStamina != args.MaxStamina)
                {
                    StaminaExt.Visibility = Visibility.Visible;
                    float maxStamina = Math.Min(args.MaxStamina + StaminaComponent.CanIncreaseMaxStamina[args.SelectedItemId], args.MaxPossibleStamina);
                    double extWidth = HealthBar.ConstantWidth * ((maxStamina - args.MaxStamina) / HealthBar.CHealth);
                    StaminaExt.Width = Math.Max(0, extWidth);
                } else
                {
                    StaminaExt.Width = 0;
                    StaminaExt.Visibility = Visibility.Collapsed;
                }
            }));
        }

        private void OnWorldDayTimeUpdate(object source, WorldEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                string iconPath = "pack://siteoforigin:,,,/HunterPie.Resources/UI/HUD/";
                switch (args.DayTime)
                {
                    case DayTime.Morning:
                        DayTimeIcon = $"{iconPath}mr_time_morning.png";
                        break;
                    case DayTime.Afternoon:
                        DayTimeIcon = $"{iconPath}mr_time_day.png";
                        break;
                    case DayTime.Evening:
                        DayTimeIcon = $"{iconPath}mr_time_evening.png";
                        break;
                    case DayTime.Night:
                        DayTimeIcon = $"{iconPath}mr_time_night.png";
                        break;
                    default:
                        DayTimeIcon = null;
                        break;
                }
            }));
        }

        private void OnClassChange(object source, EventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                if (Context.LastWeapon != null)
                {
                    Context.LastWeapon.OnSharpnessChange -= OnSharpnessChange;
                    Context.LastWeapon.OnSharpnessLevelChange -= OnSharpnessLevelChange;
                }
                // To avoid hooking twice
                Context.CurrentWeapon.OnSharpnessChange -= OnSharpnessChange;
                Context.CurrentWeapon.OnSharpnessLevelChange -= OnSharpnessLevelChange;
                Context.CurrentWeapon.OnSharpnessChange += OnSharpnessChange;
                Context.CurrentWeapon.OnSharpnessLevelChange += OnSharpnessLevelChange;

                SharpnessVisibility = Context.CurrentWeapon.IsMelee ? Visibility.Visible : Visibility.Collapsed;

                OnSharpnessLevelChange(this, new SharpnessEventArgs(Context.CurrentWeapon));
            }));
        }

        private void OnSharpnessLevelChange(object source, SharpnessEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                object downColor = TryFindResource($"SHARPNESS_{(args.Level - 1).ToString().ToUpperInvariant()}");
                object color = TryFindResource($"SHARPNESS_{(args.Level).ToString().ToUpperInvariant()}");

                if (color != null)
                {
                    SharpnessColor = color as Brush;
                }

                int min = Math.Min(args.MaximumSharpness, args.Max);

                SharpnessLowerColor = downColor as Brush;

                PlayerMinSharpness = args.Min;
                PlayerCurrentSharpness = args.Sharpness;
                PlayerMaxSharpness = min;
                PlayerSharpnessLeft = PlayerCurrentSharpness - PlayerMinSharpness;

                Sharpness = ((args.Sharpness - args.Min) / (double)(min - args.Min)) * SharpnessMaxWidth;
            }));
        }

        private void OnSharpnessChange(object source, SharpnessEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                int min = Math.Min(args.MaximumSharpness, args.Max);

                PlayerMinSharpness = args.Min;
                PlayerCurrentSharpness = args.Sharpness;
                PlayerMaxSharpness = min;
                PlayerSharpnessLeft = PlayerCurrentSharpness - PlayerMinSharpness;

                Sharpness = ((args.Sharpness - args.Min) / (double)(min - args.Min)) * SharpnessMaxWidth;
            }));
        }

        private void OnLevelChange(object source, EventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                PlayerEventArgs e = (PlayerEventArgs)args;
                // We update the name string
                PlayerName = FormatNameString();
                // And the laurel
                string laurel = "pack://siteoforigin:,,,/HunterPie.Resources/UI/HUD/";

                switch (Player.GetLaurelFromLevel(e.MasterRank))
                {
                    case Laurel.Copper:
                        laurel += "mr_laurel_copper";
                        break;
                    case Laurel.Silver:
                        laurel += "mr_laurel_silver";
                        break;
                    case Laurel.Gold:
                        laurel += "mr_laurel_gold";
                        break;
                    case Laurel.Diamond:
                        laurel += "mr_laurel_diamond";
                        break;
                    default:
                        laurel += "mr_laurel_iron";
                        break;
                }
                laurel += ".png";
                PlayerLaurel = laurel;
            }));
        }

        private string FormatNameString()
        {
            if (Context is null)
            {
                return null;
            }

            return settings.NameTextFormat.Replace("{HR}", Context.Level.ToString())
                .Replace("{MR}", Context.MasterRank.ToString())
                .Replace("{Name}", Context.Name);

        }

        private void OnAilmentUpdate(object source, PlayerAilmentEventArgs args)
        {
            if (ConstantAilment == null)
            {
                // Minimalistic widget might not have it
                return;
            }
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                if (args.AilmentType == PlayerAilment.None)
                {
                    ConstantAilment.Icon = null;
                    ConstantAilment.Visibility = Visibility.Collapsed;
                    return;
                } else
                {
                    ConstantAilment.Visibility = Visibility.Visible;
                    ImageSource icon = TryFindResource($"ICON_{args.AilmentType.ToString().ToUpperInvariant()}") as ImageSource;
                    ConstantAilment.Icon = icon;
                }

                ConstantAilment.TimerEndAngle = Arc.ConvertPercentageIntoAngle(args.AilmentTimer / Math.Max(1, args.AilmentMaxTimer));
            }));
        }

        private void OnMaxStaminaUpdate(object source, PlayerStaminaEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                PlayerCurrentStamina = (int)args.Stamina;
                PlayerMaxStamina = (int)args.MaxStamina;

                StaminaBar.MaxWidth = args.MaxStamina / 100 * HealthBar.ConstantWidth;
                StaminaBar.MaxSize = args.MaxStamina / 100 * HealthBar.ConstantWidth;

                OnStaminaExtStateUpdate(source, args);
            }));
        }

        private void OnStaminaUpdate(object source, PlayerStaminaEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                PlayerCurrentStamina = (int)args.Stamina;
                PlayerMaxStamina = (int)args.MaxStamina;

                StaminaBar.UpdateBar(args.Stamina, args.MaxStamina);
            }));
        }

        private void OnRedHealthUpdate(object source, PlayerHealthEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                HealthBar.RedHealth = args.RedHealth;
            }));
        }

        private void OnHealHealth(object source, PlayerHealthEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                double healValue = args.HealingData.Stage == 1 ?
                    args.HealingData.MaxHeal * 2.5 : args.HealingData.MaxHeal;

                if (args.HealingData.Stage == 0)
                {
                    HealthBar.HealHealth = 0;
                } else
                {
                    HealthBar.HealHealth = Math.Min(args.Health + (healValue - args.HealingData.CurrentHeal), args.MaxHealth);
                }
            }));
        }

        private void OnMaxHealthUpdate(object source, PlayerHealthEventArgs args)
        {
            bool hasMoxie = Context?.FoodData.Skills?.Contains(FoodSkills.FelyneMoxie) ?? false;
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {

                MoxieVisibility = hasMoxie ? Visibility.Visible : Visibility.Hidden;

                double calculatedLeft =  60 * (HealthBar.ConstantWidth / HealthBar.CHealth);

                MoxieMargin = new Thickness(HealthBarPanel.Margin.Left + calculatedLeft,
                    HealthBarPanel.Margin.Top, 0, 0);

                PlayerCurrentHealth = (int)args.Health;
                PlayerMaxHealth = (int)args.MaxHealth;

                HealthBar.MaxHealth = args.MaxHealth;
                HealthBar.Health = args.Health;

                OnHealthExtStateUpdate(source, args);
            }));
        }

        private void OnHealthUpdate(object source, PlayerHealthEventArgs args)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(() =>
            {
                PlayerCurrentHealth = (int)args.Health;
                PlayerMaxHealth = (int)args.MaxHealth;

                HealthBar.MaxHealth = args.MaxHealth;
                HealthBar.Health = args.Health;
                if (args.RedHealth > 0)
                {
                    HealthBar.RedHealth = args.RedHealth;
                }
            }));
        }

        public override void ScaleWidget(double NewScaleX, double NewScaleY)
        {
            if (NewScaleX <= 0.2) return;
            FrameworkElement panel = (FrameworkElement)Template.FindName("HudPanel", this);
            if (panel is null)
            {
                return;
            }
            panel.LayoutTransform = new ScaleTransform(NewScaleX, NewScaleY);
            DefaultScaleX = NewScaleX;
            DefaultScaleY = NewScaleY;
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            UnhookEvents();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            StaminaBar = Template.FindName("StaminaBar", this) as MinimalHealthBar;
            HealthBar = Template.FindName("HealthBar", this) as HealthBar;
            HealthExt = Template.FindName("HealthExt", this) as Rectangle;
            StaminaExt = Template.FindName("StaminaExt", this) as Rectangle;

            HealthBarPanel = Template.FindName("HealthBarPanel", this) as Panel;
            ConstantAilment = Template.FindName("ConstantAilment", this) as AilmentControl;

            HookEvents();
            UpdateInformation();
            ApplySettings();
        }

        private void OnInitialized(object sender, EventArgs e)
        {
            Show();
        }
    }
}
