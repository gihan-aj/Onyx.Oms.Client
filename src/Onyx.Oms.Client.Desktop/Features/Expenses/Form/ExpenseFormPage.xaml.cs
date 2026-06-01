using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

namespace Onyx.Oms.Client.Desktop.Features.Expenses.Form;

public sealed partial class ExpenseFormPage : Page
{
    public ExpenseFormViewModel ViewModel { get; }

    public ExpenseFormPage()
    {
        ViewModel = App.Current.Services.GetRequiredService<ExpenseFormViewModel>();
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (ViewModel is Shared.Services.INavigationAware navAware)
            navAware.OnNavigatedTo(e.Parameter);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        if (ViewModel is Shared.Services.INavigationAware navAware)
            navAware.OnNavigatedFrom();
    }

    // ── Add custom category flyout ────────────────────────────────────────────

    private void OnAddCategoryClick(object sender, RoutedEventArgs e)
    {
        // Clear the text box each time the flyout opens
        NewCategoryTextBox.Text = string.Empty;
    }

    private void OnNewCategoryKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
            CommitNewCategory();
    }

    private void OnConfirmAddCategoryClick(object sender, RoutedEventArgs e)
    {
        CommitNewCategory();
    }

    private void CommitNewCategory()
    {
        var text = NewCategoryTextBox.Text?.Trim();
        if (!string.IsNullOrEmpty(text))
        {
            ViewModel.AddCustomCategory(text);
            AddCategoryFlyout.Hide();
            NewCategoryTextBox.Text = string.Empty;
        }
    }
}
