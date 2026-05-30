using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Onyx.Oms.Client.Desktop.Features.Dashboard;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<DashboardViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.Subscribe();
        await ViewModel.InitializeAsync();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        ViewModel.Unsubscribe();
    }

    private ActionRequiredUIItem? GetActionItemFromSender(object sender)
    {
        if (sender is FrameworkElement { DataContext: ActionRequiredUIItem item }) return item;
        return null;
    }

    private void ActionRequiredItem_Click(object sender, RoutedEventArgs e)
        => ViewModel.NavigateToOrderCommand.Execute(GetActionItemFromSender(sender));

    private void ScrollViewer_PointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer && scrollViewer.ScrollableWidth > 0)
        {
            var pointerPoint = e.GetCurrentPoint(scrollViewer);
            if (!pointerPoint.Properties.IsHorizontalMouseWheel)
            {
                double delta = pointerPoint.Properties.MouseWheelDelta;
                double targetOffset = scrollViewer.HorizontalOffset - delta;
                targetOffset = System.Math.Max(0, System.Math.Min(targetOffset, scrollViewer.ScrollableWidth));
                
                scrollViewer.ChangeView(targetOffset, null, null);
                e.Handled = true;
            }
        }
    }
}
