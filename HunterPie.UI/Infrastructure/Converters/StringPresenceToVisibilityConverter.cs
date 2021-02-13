using System.Windows;

namespace HunterPie.UI.Infrastructure.Converters
{
    public class StringPresenceToVisibilityConverter : GenericValueConverter<string, Visibility>
    {
        public Visibility IfEmpty { get; set; } = Visibility.Collapsed;

        public Visibility IfHasValue { get; set; } = Visibility.Visible;

        public override Visibility Convert(string value, object parameter) =>
            string.IsNullOrEmpty(value) ? IfEmpty : IfHasValue;
    }
}
