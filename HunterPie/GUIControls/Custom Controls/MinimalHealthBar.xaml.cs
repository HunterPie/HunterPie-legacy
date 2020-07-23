using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HunterPie.GUIControls.Custom_Controls
{
    /// <summary>
    /// Interaction logic for MinimalHealthBar.xaml
    /// </summary>
    public partial class MinimalHealthBar : UserControl
    {
        private double valueProperty;

        public double MaxSize { get; set; }
        public double MaxValue { get; set; }
        public double Value
        {
            get => valueProperty;
            set
            {
                valueProperty = value;
                double v = Math.Max(MaxSize * (value / Math.Max(MaxValue, 1)), 0);
                HealthBar.Width = v;
            }
        }



        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush), typeof(MinimalHealthBar));

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }
        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(MinimalHealthBar));



        public MinimalHealthBar() => InitializeComponent();

        public void UpdateBar(float hp, float maxHp)
        {
            HealthBarBackground.Width = MaxSize;
            MaxValue = maxHp;
            Value = hp;
        }
    }
}
