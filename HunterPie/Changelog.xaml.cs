using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for Changelog.xaml
    /// </summary>
    public partial class Changelog : UserControl {
        private static Changelog _Instance;
        public static Changelog Instance {
            get {
                if (_Instance == null) {
                    _Instance = new Changelog();
                }
                return _Instance;
            }
        }

        public Changelog() {
            InitializeComponent();
            LoadChangelogText();
        }

        private void LoadChangelogText() {
            if (!File.Exists("changelog.log")) {
                ChangelogBox.AppendText("Changelog not found!");
                return;
            }
            string[] _changelog = File.ReadAllLines("changelog.log");
            foreach (string line in _changelog) {
                ChangelogBox.AppendText($"{line}\n");
            }
        }
    }
}
