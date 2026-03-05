using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Onyx.Oms.Client.Desktop.Shared.Services;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public sealed partial class CreateProductPage : Page
{
    public CreateProductViewModel ViewModel { get; }

    public CreateProductPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<CreateProductViewModel>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        CategoryPicker.FetchDataDelegate = ViewModel.FetchCategoriesAsync;
        ViewModel.OnNavigatedTo(e.Parameter);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        ViewModel.OnNavigatedFrom();
    }
}
