using System.Windows;

namespace HunterPie.UI.Infrastructure.Converters
{
    public class LengthToVisibilityConverter : GenericValueConverter<int, Visibility>
    {
        public Visibility WhenEmpty { get; set; } = Visibility.Collapsed;
        public Visibility WhenValues { get; set; } = Visibility.Visible;
        public override Visibility Convert(int value, object parameter) => value > 0 ? WhenValues : WhenEmpty;
    }
}
