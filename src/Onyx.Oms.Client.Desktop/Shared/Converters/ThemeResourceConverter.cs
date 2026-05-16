using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Onyx.Oms.Client.Desktop.Shared.Converters
{
    public partial class ThemeResourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string resourceKey && Application.Current.Resources.TryGetValue(resourceKey, out var resourceValue))
            {
                return resourceValue;
            }
            return DependencyProperty.UnsetValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
