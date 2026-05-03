using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Converters
{
    public partial class OrderStatusToBackgroundConverter : IValueConverter
    {
        private SolidColorBrush SuccessBrush = (SolidColorBrush)Application.Current.Resources["SystemFillColorSuccessBackgroundBrush"];
        private SolidColorBrush WarningBrush = (SolidColorBrush)Application.Current.Resources["SystemFillColorCautionBackgroundBrush"];
        private SolidColorBrush CriticalBrush = (SolidColorBrush)Application.Current.Resources["SystemFillColorCriticalBackgroundBrush"];
        private SolidColorBrush NeutralBrush = (SolidColorBrush)Application.Current.Resources["SystemFillColorNeutralBackgroundBrush"];
        private SolidColorBrush AccentBrush = (SolidColorBrush)Application.Current.Resources["SystemFillColorAttentionBackgroundBrush"]; // Often Blue
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is OrderStatus status)
            {
                return status switch
                {
                    OrderStatus.Pending => NeutralBrush,

                    // Active / In-progress states (Blue/Accent)
                    OrderStatus.Confirmed => AccentBrush,
                    OrderStatus.Processing => AccentBrush,
                    OrderStatus.ReadyToPack => AccentBrush,
                    OrderStatus.Packed => AccentBrush,
                    OrderStatus.Shipped => AccentBrush,

                    // Completed states (Green)
                    OrderStatus.Delivered => SuccessBrush,
                    OrderStatus.Completed => SuccessBrush,

                    // Error/Cancelled states (Red)
                    OrderStatus.PaymentFailed => CriticalBrush,
                    OrderStatus.Cancelled => CriticalBrush,
                    OrderStatus.ReturnedToSender => CriticalBrush,
                    OrderStatus.DeliveryFailed => CriticalBrush,

                    _ => NeutralBrush
                };
            }
            return NeutralBrush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
