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
using System.Windows.Shapes;

namespace HunterPie.GUI.Widgets.Notification_Widget {
    /// <summary>
    /// Interaction logic for DiscordNotify.xaml
    /// </summary>
    public partial class DiscordNotify : Widget {
        public DiscordNotify() {
            this.WidgetActive = true;
            this.WidgetHasContent = true;
            InitializeComponent();
        }
    }
}
