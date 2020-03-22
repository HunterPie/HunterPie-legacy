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

namespace HunterPie.GUIControls.Custom_Controls {
    /// <summary>
    /// Interaction logic for MinimalHealthBar.xaml
    /// </summary>
    public partial class MinimalHealthBar : UserControl {
        private double _MaxHealth { get; set; }
        private double _Health { get; set; }
        public double MaxSize { get; set; }

        public double MaxHealth {
            get { return _MaxHealth; }
            set {
                _MaxHealth = value;
            }
        }

        public double Health {
            get { return _Health; }
            set {
                _Health = value;
                HealthBar.Width = MaxSize * (value / MaxHealth) > 0 ? MaxSize * (value / MaxHealth) : 0;
            }
        }

        public Brush Color {
            get { return HealthBar.Fill; }
            set { HealthBar.Fill = value; }
        }

        public MinimalHealthBar() {
            InitializeComponent();
        }

        public void UpdateBar(float hp, float max_hp) {
            HealthBarBackground.Width = MaxSize;
            this.MaxHealth = max_hp;
            this.Health = hp;
        }
    }
}
