using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.ProductPicker
{
    public sealed partial class ProductPickerDialog : ContentDialog
    {
        public ProductPickerViewModel ViewModel { get; }

        public ProductPickerDialog()
        {
            InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<ProductPickerViewModel>();
            DataContext = ViewModel;

            CategoryPicker.FetchDataDelegate = ViewModel.FetchCategoriesAsync;

            Loaded += (s, e) => ViewModel.LoadDataCommand.Execute(null);
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ViewModel.LoadDataCommand.Execute(null);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
    }
}
