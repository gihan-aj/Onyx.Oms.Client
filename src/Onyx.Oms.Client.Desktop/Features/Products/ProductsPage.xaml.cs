using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Onyx.Oms.Client.Desktop.Shared.Models;
using System.Linq;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public sealed partial class ProductsPage : Page
{
    public ProductsViewModel ViewModel { get; }

    public ProductsPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ProductsViewModel>();
        DataContext = ViewModel;
        
        // Wire up the delegate manually so it can securely access the view model DI bounds
        CategoryPicker.FetchDataDelegate = ViewModel.FetchCategoriesAsync;

        // Sync initial button state per permissions constraint detailed in the guide
        NewProductButton.Visibility = ViewModel.CanCreateProduct ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        ViewModel.LoadDataCommand.Execute(null);
    }

    private void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        // Reset visual sort indicators
        foreach (var column in ProductsDataGrid.Columns.OfType<DataGridBoundColumn>())
        {
            column.SortDirection = null;
        }

        ViewModel.RefreshCommand.Execute(null);
    }

    private async void OnDataGridSorting(object sender, DataGridColumnEventArgs e)
    {
        var tag = e.Column.Tag?.ToString();
        if (string.IsNullOrEmpty(tag)) return;

        // Reset other columns visually
        foreach (var column in ProductsDataGrid.Columns.OfType<DataGridBoundColumn>())
        {
            if (column != e.Column)
            {
                column.SortDirection = null;
            }
        }

        // Determine new sort direction visually
        if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
        {
            e.Column.SortDirection = DataGridSortDirection.Ascending;
        }
        else
        {
            e.Column.SortDirection = DataGridSortDirection.Descending;
        }

        // Execute API call via viewmodel
        var sortOrder = e.Column.SortDirection == DataGridSortDirection.Ascending ? "asc" : "desc";
        await ViewModel.SortByAsync(tag, sortOrder);
    }

    private async void OnActivateClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ProductDto product })
        {
            await ViewModel.ActivateProductAsync(product);
        }
    }

    private async void OnDeactivateClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ProductDto product })
        {
            await ViewModel.DeactivateProductAsync(product);
        }
    }

    private void OnViewClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ProductDto product })
        {
            ViewModel.ViewDetailsCommand.Execute(product);
        }
    }

    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ProductDto product })
        {
            ViewModel.EditDetailsCommand.Execute(product);
        }
    }
}
