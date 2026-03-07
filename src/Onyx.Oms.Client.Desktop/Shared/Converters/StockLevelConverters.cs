using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace Onyx.Oms.Client.Desktop.Shared.Converters;

public partial class StockLevelToBackgroundConverter : IValueConverter
{
    public SolidColorBrush SuccessBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["SystemFillColorSuccessBackgroundBrush"];
    public SolidColorBrush WarningBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["SystemFillColorCautionBackgroundBrush"];
    public SolidColorBrush DangerBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["SystemFillColorCriticalBackgroundBrush"];

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int stock)
        {
            if (stock > 10) return SuccessBrush;
            if (stock > 0) return WarningBrush;
            return DangerBrush;
        }
        return Application.Current.Resources["LayerFillColorAltBrush"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public partial class StockLevelToForegroundConverter : IValueConverter
{
    public SolidColorBrush SuccessBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["SystemFillColorSuccessBrush"];
    public SolidColorBrush WarningBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["SystemFillColorCautionBrush"];
    public SolidColorBrush DangerBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["SystemFillColorCriticalBrush"];

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int stock)
        {
            if (stock > 10) return SuccessBrush;
            if (stock > 0) return WarningBrush;
            return DangerBrush;
        }
        return Application.Current.Resources["TextFillColorPrimaryBrush"];
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}

public partial class StockLevelToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int stock)
        {
            if (stock > 10) return "\uE814"; // E814 CheckMark
            if (stock > 0) return "\uE814"; // Will just use checkmark for warn too, or maybe \uE7BA for warning
            return "\uE711"; // E711 Cancel (X)
        }
        return "\uE814";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
