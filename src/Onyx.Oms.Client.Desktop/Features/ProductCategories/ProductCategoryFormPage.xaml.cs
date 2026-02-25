using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Onyx.Oms.Client.Desktop.Features.ProductCategories;

public sealed partial class ProductCategoryFormPage : Page
{
    public ProductCategoryFormViewModel ViewModel { get; }

    public ProductCategoryFormPage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ProductCategoryFormViewModel>();
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
