using System.Collections;
using System.Collections.Generic;
using System.Windows;

namespace HunterPie.UI.Infrastructure.Converters
{
    public class BooleanToVisibilityConverter : GenericValueConverter<bool, Visibility>
    {
        public Visibility FalseValue { get; set; } = Visibility.Hidden;
        public Visibility TrueValue { get; set; } = Visibility.Visible;

        public override Visibility Convert(bool value, object parameter)
        {
            return value ? this.TrueValue : this.FalseValue;
        }
    }
}
