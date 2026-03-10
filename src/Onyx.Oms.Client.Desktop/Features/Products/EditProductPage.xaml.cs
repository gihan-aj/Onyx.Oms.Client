using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Onyx.Oms.Client.Desktop.Features.Products
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditProductPage : Page
    {
        public EditProductViewModel ViewModel { get; }
        public EditProductPage()
        {
            InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<EditProductViewModel>();
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

        private async void HasVariantsToggle_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggleSwitch && ViewModel.Product != null)
            {
                // Prevent duplicate calls if the UI is just syncing with the ViewModel
                if (ViewModel.Product.HasVariants == toggleSwitch.IsOn) return;
                
                // ToggleSwitch.IsOn reflects the new state the user intends
                await ViewModel.ToggleHasVariantsCommand.ExecuteAsync(toggleSwitch.IsOn);
            }
        }
    }

}
