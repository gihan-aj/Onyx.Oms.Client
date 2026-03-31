using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;

namespace Onyx.Oms.Client.Desktop.Shared.Converters;

public partial class StringListConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is IEnumerable<string> stringList)
        {
            return string.Join(", ", stringList);
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException("StringListConverter only supports OneWay binding.");
    }
}
