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

namespace Onyx.Oms.Client.Desktop.Features.Products.Edit
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
            ViewModel.OnNavigatedTo(e.Parameter);
            CategoryPicker.FetchDataDelegate = ViewModel.FetchCategoriesAsync;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ViewModel.OnNavigatedFrom();
        }

        private async void HasVariantsToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if(sender is ToggleSwitch toggleSwitch && ViewModel.BasicInfo != null)
            {
                if (ViewModel.HasVariants == toggleSwitch.IsOn)
                    return;

                await ViewModel.ToggleVariantsCommand.ExecuteAsync(toggleSwitch.IsOn);
            }
        }
    }
}
