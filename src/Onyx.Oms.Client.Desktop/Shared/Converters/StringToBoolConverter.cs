using Microsoft.UI.Xaml.Data;
using System;

namespace Onyx.Oms.Client.Desktop.Shared.Converters;

public class StringToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string strValue)
        {
            if (bool.TryParse(strValue, out bool bValue))
                return bValue;
            
            if (strValue.Equals("yes", StringComparison.OrdinalIgnoreCase) || strValue.Equals("1"))
                return true;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool bValue)
        {
            return bValue ? "true" : "false";
        }
        return "false";
    }
}
