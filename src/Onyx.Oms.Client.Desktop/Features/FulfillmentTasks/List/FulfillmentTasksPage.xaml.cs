using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.List
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FulfillmentTasksPage : Page
    {
        public FulfillmentTasksViewModel ViewModel { get; }
        public FulfillmentTasksPage()
        {
            InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<FulfillmentTasksViewModel>();
            DataContext = ViewModel;
        }

        private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ViewModel.LoadDataCommand.Execute(null);
        }

        private async void OnDataGridSorting(object sender, DataGridColumnEventArgs e)
        {
            var tag = e.Column.Tag?.ToString();
            if (string.IsNullOrEmpty(tag))
                return;

            foreach (var column in TasksDataGrid.Columns.OfType<DataGridBoundColumn>())
            {
                if (column != e.Column)
                    column.SortDirection = null;
            }

            var sortOrder = e.Column.SortDirection == DataGridSortDirection.Ascending ? "asc" : "desc";
            await ViewModel.SortByAsync(tag, sortOrder);
        }

        private void OnActionButtonTapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // 1. Stop the DataGrid from stealing this click to select the row
            e.Handled = true;

            // 2. Manually trigger the flyout
            if (sender is Button btn && btn.Flyout != null)
            {
                btn.Flyout.ShowAt(btn);
            }
        }

        private async void OnViewClick(object sender, RoutedEventArgs e)
        {
            //if ((sender as FrameworkElement)?.DataContext is CustomerDto customer)
            //{
            //    var dialog = new CustomerDetailsDialog(customer);
            //    if (dialog != null)
            //    {
            //        dialog.XamlRoot = this.XamlRoot;
            //        await dialog.ShowAsync();
            //    }
            //}
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            //if ((sender as FrameworkElement)?.DataContext is CustomerDto customer)
            //{
            //    ViewModel.EditCustomerCommand.Execute(customer);
            //}
        }
    }
}
