using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Converters
{
    public partial class PaymentStatusToForegroundConverter : IValueConverter
    {
        private SolidColorBrush SuccessBrush = (SolidColorBrush)Application.Current.Resources["SystemFillColorSuccessBrush"];
        private SolidColorBrush WarningBrush = (SolidColorBrush)Application.Current.Resources["SystemFillColorCautionBrush"];
        private SolidColorBrush CriticalBrush = (SolidColorBrush)Application.Current.Resources["SystemFillColorCriticalBrush"];
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PaymentStatus status)
            {
                return status switch
                {
                    PaymentStatus.Unpaid => CriticalBrush,
                    PaymentStatus.PartiallyPaid => WarningBrush,
                    PaymentStatus.FullyPaid => SuccessBrush,
                    _ => CriticalBrush
                };
            }
            return CriticalBrush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
