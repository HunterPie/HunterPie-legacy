using System;
using System.Globalization;
using System.Windows.Data;

namespace HunterPie.GUI.Helpers {
    class PercentageConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            double ProgressValue = System.Convert.ToDouble(values[0]);
            double ProgressMaximum = System.Convert.ToDouble(values[1]);
            return String.Format("{0:0.00}/{1:0.00} ({2:0}%)", ProgressValue, ProgressMaximum, (ProgressValue / ProgressMaximum) * 100);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
