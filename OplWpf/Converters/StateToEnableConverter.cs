using OplWpf.Models;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace OplWpf.Converters;

public class StateToEnableConverter : MarkupExtension, IValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is State state)
        {
            return state switch
            {
                State.Stop => true,
                _ => false
            };
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
