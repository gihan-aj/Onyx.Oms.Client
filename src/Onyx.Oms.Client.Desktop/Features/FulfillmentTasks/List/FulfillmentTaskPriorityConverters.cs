using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List
{
    public partial class FulfillmentTaskPriorityToBackgroundConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TaskPriority priority)
            {
                return priority switch
                {
                    TaskPriority.Low => Application.Current.Resources["SystemFillColorNeutralBackgroundBrush"],
                    TaskPriority.Normal => Application.Current.Resources["SystemFillColorSolidNeutralBackgroundBrush"],
                    TaskPriority.High => Application.Current.Resources["SystemFillColorAttentionBackgroundBrush"],
                    TaskPriority.Urgent => Application.Current.Resources["SystemFillColorCriticalBackgroundBrush"],
                    _ => Application.Current.Resources["SystemFillColorNeutralBackgroundBrush"]
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }

    public partial class FulfillmentTaskPriorityToForegroundConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TaskPriority priority)
            {
                return priority switch
                {
                    TaskPriority.Low => Application.Current.Resources["TextFillColorSecondaryBrush"],
                    TaskPriority.Normal => Application.Current.Resources["TextFillColorPrimaryBrush"],
                    TaskPriority.High => Application.Current.Resources["SystemFillColorAttentionBrush"],
                    TaskPriority.Urgent => Application.Current.Resources["SystemFillColorCriticalBrush"],
                    _ => Application.Current.Resources["TextFillColorPrimaryBrush"]
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
