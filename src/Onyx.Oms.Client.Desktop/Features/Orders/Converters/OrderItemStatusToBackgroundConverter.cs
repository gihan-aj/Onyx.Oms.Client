using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Converters
{
    public partial class OrderItemStatusToBackgroundConverter : IValueConverter
    {
        public SolidColorBrush SuccessBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["SystemFillColorSuccessBackgroundBrush"];
        public SolidColorBrush WarningBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["SystemFillColorCautionBackgroundBrush"];
        public SolidColorBrush NeutralBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["SystemFillColorNeutralBackgroundBrush"];
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
