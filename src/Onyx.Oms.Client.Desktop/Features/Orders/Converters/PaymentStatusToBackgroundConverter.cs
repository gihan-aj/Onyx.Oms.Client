using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Converters
{
    public partial class PaymentStatusToBackgroundConverter : IValueConverter
    {
        private SolidColorBrush SuccessBrush = (SolidColorBrush)Application.Current.Resources["SystemFillColorSuccessBackgroundBrush"];
        private SolidColorBrush WarningBrush = (SolidColorBrush)Application.Current.Resources["SystemFillColorCautionBackgroundBrush"];
        private SolidColorBrush CriticalBrush = (SolidColorBrush)Application.Current.Resources["SystemFillColorCriticalBackgroundBrush"];
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PaymentStatus status)
            {
                return status switch
                {
                    PaymentStatus.Unpaid => CriticalBrush,       // Red
                    PaymentStatus.PartiallyPaid => WarningBrush, // Yellow
                    PaymentStatus.FullyPaid => SuccessBrush,     // Green
                    _ => CriticalBrush
                };
            }
            return CriticalBrush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
