using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Converters
{
    public partial class OrderItemStatusToForegroundConverter : IValueConverter
    {
        public SolidColorBrush SuccessBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["SystemFillColorSuccessBrush"];
        public SolidColorBrush WarningBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["SystemFillColorCautionBrush"];
        public SolidColorBrush NeutralBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["TextFillColorSecondaryBrush"];
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is OrderItemStatus status)
            {
                return status switch
                {
                    OrderItemStatus.Ready => SuccessBrush,
                    OrderItemStatus.Allocated => SuccessBrush,
                    OrderItemStatus.Pending => WarningBrush,
                    _ => NeutralBrush // InProduction, Ordered
                };
            }
            return NeutralBrush;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
