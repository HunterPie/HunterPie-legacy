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
using HunterPie.Logger;
using Newtonsoft.Json;

namespace HunterPie.GUI.Widgets.HealthWidget.Parts
{
    /// <summary>
    /// Interaction logic for HealthBar.xaml
    /// </summary>
    public partial class HealthBar : UserControl
    {
        private const double CWidth = 200.0;
        private const double CHealth = 100.0;

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
            InitializeComponent();
        }
    }
}
