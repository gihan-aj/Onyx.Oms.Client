using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List
{
    public partial class FulfillmentTaskStatusToBackgroundConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is FulfillmentTaskStatus status)
            {
                return status switch
                {
                    FulfillmentTaskStatus.Pending => Application.Current.Resources["SystemFillColorNeutralBackgroundBrush"],
                    FulfillmentTaskStatus.InProgress => Application.Current.Resources["SystemFillColorSolidNeutralBackgroundBrush"],
                    FulfillmentTaskStatus.Ready => Application.Current.Resources["SystemFillColorSuccessBackgroundBrush"],
                    FulfillmentTaskStatus.Cancelled => Application.Current.Resources["SystemFillColorCriticalBackgroundBrush"],
                    _ => Application.Current.Resources["SystemFillColorNeutralBackgroundBrush"]
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }

    public partial class FulfillmentTaskStatusToForegroundConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is FulfillmentTaskStatus status)
            {
                return status switch
                {
                    FulfillmentTaskStatus.Pending => Application.Current.Resources["TextFillColorSecondaryBrush"],
                    FulfillmentTaskStatus.InProgress => Application.Current.Resources["SystemFillColorAttentionBrush"],
                    FulfillmentTaskStatus.Ready => Application.Current.Resources["SystemFillColorSuccessBrush"],
                    FulfillmentTaskStatus.Cancelled => Application.Current.Resources["SystemFillColorCriticalBrush"],
                    _ => Application.Current.Resources["TextFillColorPrimaryBrush"]
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
