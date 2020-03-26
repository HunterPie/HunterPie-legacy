using System;
using System.Globalization;
using System.Windows.Data;

namespace HunterPie.GUI.Helpers {
    class StaticSecondsConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return $"{value} seconds";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
