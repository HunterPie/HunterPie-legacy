using System.Windows.Controls;
using System.Windows.Media;

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
