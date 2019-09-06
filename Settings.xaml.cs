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

namespace HunterPie {
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : UserControl {

        public static Settings _Instance;
        public static Settings Instance {
            get {
                if (_Instance == null) {
                    _Instance = new Settings();
                }
                return _Instance;
            }
        }

        public Settings() {
            InitializeComponent();
        }
    }
}
