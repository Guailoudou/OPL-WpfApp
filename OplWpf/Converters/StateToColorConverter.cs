using OplWpf.Models;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace OplWpf.Converters;

public class StateToColorConverter : MarkupExtension, IValueConverter
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
                State.Stop => Brushes.Gray,
                State.Loading => Brushes.Orange,
                State.Running => Brushes.Green,
                _ => Brushes.Gray
            };
        }

        return Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}

public class AppToColorConverter : MarkupExtension, IMultiValueConverter
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is [string protocol, int srcPort, Dictionary<string, State> appState])
        {
            var state = appState.GetValueOrDefault(protocol + ':' + srcPort, State.Stop);
            return state switch
            {
                State.Stop => Brushes.Gray,
                State.Loading => Brushes.Orange,
                State.Running => Brushes.Green,
                _ => Brushes.Gray
            };
        }

        return Brushes.Gray;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        return [Binding.DoNothing];
    }
}