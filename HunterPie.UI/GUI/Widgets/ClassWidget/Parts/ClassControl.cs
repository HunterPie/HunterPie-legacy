using System.Windows;
using System.Windows.Controls;

namespace HunterPie.GUI.Widgets.ClassWidget.Parts
{
    public class ClassControl : UserControl
    {

        public int SafiCounter
        {
            get => (int)GetValue(SafiCounterProperty);
            set => SetValue(SafiCounterProperty, value);
        }

        public static readonly DependencyProperty SafiCounterProperty =
            DependencyProperty.Register("SafiCounter", typeof(int), typeof(ClassControl));

        public bool HasSafiBuff
        {
            get => (bool)GetValue(HasSafiBuffProperty);
            set => SetValue(HasSafiBuffProperty, value);
        }

        public static readonly DependencyProperty HasSafiBuffProperty =
            DependencyProperty.Register("HasSafiBuff", typeof(bool), typeof(ClassControl));

        public bool IsWeaponSheathed
        {
            get => (bool)GetValue(IsWeaponSheathedProperty);
            set => SetValue(IsWeaponSheathedProperty, value);
        }

        public static readonly DependencyProperty IsWeaponSheathedProperty =
            DependencyProperty.Register("IsWeaponSheathed", typeof(bool), typeof(ClassControl));

        public virtual void UnhookEvents() { }
    }
}
