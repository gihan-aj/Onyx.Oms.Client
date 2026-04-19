using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.Edit
{
    public sealed partial class EditFulfillmentTaskPage : Page
    {
        public EditFulfillmentTaskViewModel ViewModel { get; }

        public EditFulfillmentTaskPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<EditFulfillmentTaskViewModel>();
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
}
