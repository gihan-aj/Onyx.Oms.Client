using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

public sealed partial class CourierFormPage : Page
{
    public CourierFormViewModel ViewModel { get; }

    public CourierFormPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<CourierFormViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (ViewModel is Shared.Services.INavigationAware navAware)
        {
            navAware.OnNavigatedTo(e.Parameter);
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        if (ViewModel is Shared.Services.INavigationAware navAware)
        {
            navAware.OnNavigatedFrom();
        }
    }
}
