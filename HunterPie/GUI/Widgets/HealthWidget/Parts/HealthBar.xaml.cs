using System;
using System.Windows;
using System.Windows.Controls;

namespace HunterPie.GUI.Widgets.HealthWidget.Parts
{
    /// <summary>
    /// Interaction logic for HealthBar.xaml
    /// </summary>
    public partial class HealthBar : UserControl
    {
        // TODO: Replace these with customizable stuff
        public const double CWidth = 200.0;
        public const double CHealth = 100.0;

        public bool IsNormal
        {
            get { return (bool)GetValue(IsNormalProperty); }
            set { SetValue(IsNormalProperty, value); }
        }
        public static readonly DependencyProperty IsNormalProperty =
            DependencyProperty.Register("IsNormal", typeof(bool), typeof(HealthBar));

        public bool IsPoisoned
        {
            get { return (bool)GetValue(IsPoisonedProperty); }
            set { SetValue(IsPoisonedProperty, value); }
        }
        public static readonly DependencyProperty IsPoisonedProperty =
            DependencyProperty.Register("IsPoisoned", typeof(bool), typeof(HealthBar));

        public bool IsBleeding
        {
            get { return (bool)GetValue(IsBleedingProperty); }
            set { SetValue(IsBleedingProperty, value); }
        }
        public static readonly DependencyProperty IsBleedingProperty =
            DependencyProperty.Register("IsBleeding", typeof(bool), typeof(HealthBar));

        public bool IsHealing
        {
            get { return (bool)GetValue(IsHealingProperty); }
            set { SetValue(IsHealingProperty, value); }
        }
        public static readonly DependencyProperty IsHealingProperty =
            DependencyProperty.Register("IsHealing", typeof(bool), typeof(HealthBar));

        public bool IsOnFire
        {
            get { return (bool)GetValue(IsOnFireProperty); }
            set { SetValue(IsOnFireProperty, value); }
        }
        public static readonly DependencyProperty IsOnFireProperty =
            DependencyProperty.Register("IsOnFire", typeof(bool), typeof(HealthBar));

        public double MaxHealth
        {
            get { return (double)GetValue(MaxHealthProperty); }
            set
            {
                // Calculates the health bar max width based on the max health
                value = CWidth * (value / CHealth);
                
                SetValue(MaxHealthProperty, value);
            }
        }
        public static readonly DependencyProperty MaxHealthProperty =
            DependencyProperty.Register("MaxHealth", typeof(double), typeof(HealthBar));

        public double Health
        {
            get { return (double)GetValue(HealthProperty); }
            set
            {
                // Calculates the health bar width based on health value
                double maxHealth = MaxHealth / CWidth * CHealth;
                value = value / maxHealth * MaxHealth;
                SetValue(HealthProperty, value);
            }
        }
        public static readonly DependencyProperty HealthProperty =
            DependencyProperty.Register("Health", typeof(double), typeof(HealthBar));

        public double RedHealth
        {
            get { return (double)GetValue(RedHealthProperty); }
            set
            {
                double maxHealth = MaxHealth / CWidth * CHealth;
                double health = Health * maxHealth / MaxHealth;
                double heal = HealHealth > 0 ? HealHealth * 100 / 200 + health : 0;

                value = Math.Max(0, (value - Math.Max(heal, health)) / CHealth * CWidth);
                
                SetValue(RedHealthProperty, value);
            }
        }
        public static readonly DependencyProperty RedHealthProperty =
            DependencyProperty.Register("RedHealth", typeof(double), typeof(HealthBar));

        public double HealHealth
        {
            get { return (double)GetValue(HealHealthProperty); }
            set
            {
                double maxHealth = MaxHealth / CWidth * CHealth;
                double health = Health * maxHealth / MaxHealth;
                value = (value - health) / CHealth * CWidth;
                value = Math.Max(0, value);
                SetValue(HealHealthProperty, value);
            }
        }
        public static readonly DependencyProperty HealHealthProperty =
            DependencyProperty.Register("HealHealth", typeof(double), typeof(HealthBar));

        public HealthBar()
        {
            IsNormal = true;
            InitializeComponent();
        }
    }
}
