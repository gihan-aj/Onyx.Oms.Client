using Microsoft.UI.Xaml.Controls;
using System;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

public sealed partial class CourierDetailsDialog : ContentDialog
{
    public CourierDto Courier { get; }

    public Uri? WebsiteUri
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Courier?.WebsiteUrl))
            {
                return null;
            }

            string url = Courier.WebsiteUrl.Trim();
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return uri;
            }

            return null;
        }
    }

    public CourierDetailsDialog(CourierDto courier)
    {
        Courier = courier;
        InitializeComponent();
    }
}
