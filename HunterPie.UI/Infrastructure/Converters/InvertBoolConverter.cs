namespace HunterPie.UI.Infrastructure.Converters
{
    public class InvertBoolConverter : GenericValueConverter<bool, bool>
    {
        public override bool Convert(bool value, object parameter) => !value;
    }
}
