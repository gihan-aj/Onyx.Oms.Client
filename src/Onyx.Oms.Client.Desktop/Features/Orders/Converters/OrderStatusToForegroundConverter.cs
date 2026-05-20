using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Converters
{
    public partial class OrderStatusToForegroundConverter : IValueConverter
    {
        private SolidColorBrush SuccessBrush = (SolidColorBrush)Application.Current.Resources["SystemFillColorSuccessBrush"];
        private SolidColorBrush WarningBrush = (SolidColorBrush)Application.Current.Resources["SystemFillColorCautionBrush"];
        private SolidColorBrush CriticalBrush = (SolidColorBrush)Application.Current.Resources["SystemFillColorCriticalBrush"];
        private SolidColorBrush NeutralBrush = (SolidColorBrush)Application.Current.Resources["TextFillColorPrimaryBrush"];
        private SolidColorBrush AccentBrush = (SolidColorBrush)Application.Current.Resources["AccentTextFillColorPrimaryBrush"];
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is OrderStatus status)
            {
                return status switch
                {
                    OrderStatus.Pending => NeutralBrush,
                    OrderStatus.Confirmed or OrderStatus.Processing or OrderStatus.ReadyToPack or
                    OrderStatus.Packed or OrderStatus.Shipped => AccentBrush,
                    OrderStatus.Delivered or OrderStatus.Completed => SuccessBrush,
                    OrderStatus.ReturnInTransit => WarningBrush,
                    OrderStatus.PaymentFailed or OrderStatus.Cancelled or OrderStatus.LostInTransit or
                    OrderStatus.ReturnedToSender or OrderStatus.DeliveryFailed => CriticalBrush,
                    _ => NeutralBrush
                };
            }
            return NeutralBrush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
