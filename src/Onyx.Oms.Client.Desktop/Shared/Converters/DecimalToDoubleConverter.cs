using Microsoft.UI.Xaml.Data;
using System;

namespace Onyx.Oms.Client.Desktop.Shared.Converters;

public class DecimalToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal decimalValue)
        {
            return (double)decimalValue;
        }
        return 0d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is double doubleValue)
        {
            return (decimal)doubleValue;
        }
        if (value is int intValue)
        {
            return (decimal)intValue;
        }
        return 0m;
    }
}
