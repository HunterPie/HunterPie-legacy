using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HunterPie.GUIControls.Custom_Controls
{
    /// <summary>
    /// Interaction logic for CHotKey.xaml
    /// </summary>
    public partial class CHotKey : UserControl
    {


        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(CHotKey));



        public string HotKey
        {
            get => (string)GetValue(HotKeyProperty);
            set => SetValue(HotKeyProperty, value);
        }

        // Using a DependencyProperty as the backing store for HotKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HotKeyProperty =
            DependencyProperty.Register("HotKey", typeof(string), typeof(CHotKey));



        public CHotKey() => InitializeComponent();


        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Credits to this stackoverflow post I found: https://stackoverflow.com/questions/2136431/how-do-i-read-custom-keyboard-shortcut-from-user-in-wpf
            // The text box grabs all input.
            e.Handled = true;

            // Fetch the actual shortcut key.
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            // Ignore modifier keys.
            if (key == Key.LeftShift || key == Key.RightShift
                || key == Key.LeftCtrl || key == Key.RightCtrl
                || key == Key.LeftAlt || key == Key.RightAlt
                || key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            // Delete key removes the HotKey
            if (key == Key.Delete)
            {
                SetValue(HotKeyProperty, "None");
                return;
            }

            // Build the shortcut key name.
            StringBuilder shortcutText = new StringBuilder();
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                shortcutText.Append("Ctrl+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                shortcutText.Append("Shift+");
            }
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0)
            {
                shortcutText.Append("Alt+");
            }
            shortcutText.Append(key.ToString());
            SetValue(HotKeyProperty, shortcutText.ToString());
        }

    }
}
