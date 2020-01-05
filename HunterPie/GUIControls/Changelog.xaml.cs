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

namespace HunterPie.GUIControls {
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

        private string GetLineColor(string line) {
            if (line.StartsWith("+")) return "#59FF85";
            if (line.StartsWith("~")) return "#FFC13D";
            if (line.StartsWith("-")) return "#FF6459";
            return "#FFFFFF";
        }

        private void LoadChangelogText() {
            if (!File.Exists("changelog.log")) {
                ChangelogBox.AppendText("Changelog not found!");
                return;
            }
            string[] _changelog = File.ReadAllLines("changelog.log");
            foreach (string line in _changelog) {
                TextRange fLine = new TextRange(ChangelogBox.Document.ContentEnd, ChangelogBox.Document.ContentEnd);
                fLine.Text = $"{line}\n";
                if (line.StartsWith("PATCH")) {
                    fLine.ApplyPropertyValue(TextElement.FontWeightProperty, "Bold");
                } else {
                    fLine.ApplyPropertyValue(TextElement.FontWeightProperty, "Light");
                }
                fLine.ApplyPropertyValue(TextElement.ForegroundProperty, GetLineColor(line));
                
            }
        }

    }
}
