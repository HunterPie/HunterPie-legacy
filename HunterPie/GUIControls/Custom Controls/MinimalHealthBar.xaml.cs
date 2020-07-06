using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace HunterPie.GUIControls.Custom_Controls
{
    /// <summary>
    /// Interaction logic for MinimalHealthBar.xaml
    /// </summary>
    public partial class MinimalHealthBar : UserControl
    {
        private double health;

        public double MaxSize { get; set; }

        public double MaxHealth { get; set; }

        public double Health
        {
            get => health;
            set
            {
                health = value;
                double v = Math.Max(MaxSize * (value / Math.Max(MaxHealth, 1)), 0);
                HealthBar.Width = v;
            }
        }

        public Brush Color
        {
            get => HealthBar.Fill;
            set => HealthBar.Fill = value;
        }

        public MinimalHealthBar() => InitializeComponent();

        public void UpdateBar(float hp, float maxHp)
        {
            HealthBarBackground.Width = MaxSize;
            MaxHealth = maxHp;
            Health = hp;
        }
    }
}
