using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Onyx.Oms.Client.Desktop.Features.Products.List
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductsPage : Page
    {
        public ProductsViewModel ViewModel { get; set; }
        public ProductsPage()
        {
            InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<ProductsViewModel>();
            DataContext = ViewModel;

            CategoryPicker.FetchDataDelegate = ViewModel.FetchCategoriesAsync;
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ViewModel.LoadDataCommand.Execute(null);
        }

        private async void ProductsDataGrid_Sorting(object sender, DataGridColumnEventArgs e)
        {
            var tag = e.Column.Tag?.ToString();
            if (string.IsNullOrEmpty(tag))
                return;

            foreach(var column in ProductsDataGrid.Columns.OfType<DataGridBoundColumn>())
            {
                if (column != e.Column)
                    column.SortDirection = null;
            }

            var sortOrder = e.Column.SortDirection == DataGridSortDirection.Ascending ? "asc" : "desc";
            await ViewModel.SortByAsync(tag, sortOrder);
        }

        private void OnActionButtonTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            e.Handled = true;
            if (sender is Button btn && btn.Flyout != null)
            {
                btn.Flyout.ShowAt(btn);
            }
        }

        private void ViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: ProductGridItem product })
                ViewModel.ViewDetailsCommand.Execute(product);
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: ProductGridItem product })
                ViewModel.EditDetailsCommand.Execute(product);
        }

        private async void ActivateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(sender is FrameworkElement { DataContext: ProductGridItem product })
                await ViewModel.ActivateProductAsync(product);
        }

        private async void DeactivateMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: ProductGridItem product })
                await ViewModel.DeactivateProductAsync(product);
        }
    }
}
