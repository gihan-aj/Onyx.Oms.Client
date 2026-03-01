using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Onyx.Oms.Client.Desktop.Shared.Models;
using System.Linq;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public sealed partial class ProductFormPage : Page
{
    public ProductFormViewModel ViewModel { get; }

    public ProductFormPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ProductFormViewModel>();
        DataContext = ViewModel;

        // Wire up the delegate manually
        CategoryPicker.FetchDataDelegate = ViewModel.FetchCategoriesAsync;
    }

    private void OnAddImageClick(object sender, RoutedEventArgs e)
    {
        var url = ImageUrlBox.Text;
        if (string.IsNullOrWhiteSpace(url)) return;

        var color = ImageColorBox.Text;
        if (string.IsNullOrWhiteSpace(color)) color = null;

        var isMain = ImageIsMainCheck.IsChecked ?? false;

        ViewModel.Images.Add(new CreateProductImageDto(
            Url: url,
            DisplayOrder: ViewModel.Images.Count,
            IsMain: isMain,
            Color: color
        ));
        
        ViewModel.HasUnsavedChanges = true;

        // Reset
        ImageUrlBox.Text = string.Empty;
        ImageColorBox.Text = string.Empty;
        ImageIsMainCheck.IsChecked = false;
    }
}
