using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public sealed partial class ProductDetailsPage : Page
{
    public ProductDetailsViewModel ViewModel { get; }

    public ProductDetailsPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ProductDetailsViewModel>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.OnNavigatedTo(e.Parameter);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        ViewModel.OnNavigatedFrom();
    }
}
