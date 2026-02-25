using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Onyx.Oms.Client.Desktop.Features.Customers;

public sealed partial class CustomerFormPage : Page
{
    public CustomerFormViewModel ViewModel { get; }

    public CustomerFormPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<CustomerFormViewModel>();
        DataContext = ViewModel;
    }
}
