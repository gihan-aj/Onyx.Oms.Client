using Microsoft.Extensions.DependencyInjection;
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

    private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (sender.SelectedItem?.Tag is string tag)
        {
            ViewModel.SelectedFilter = tag;
            ViewModel.LoadDummyData();
        }
    }

    private void ScrollLeft_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var currentOffset = QuickActionsScrollViewer.HorizontalOffset;
        var offset = System.Math.Max(currentOffset - 256, 0); // 240 width + 16 spacing
        QuickActionsScrollViewer.ChangeView(offset, null, null);
    }

    private void ScrollRight_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var currentOffset = QuickActionsScrollViewer.HorizontalOffset;
        var maxOffset = QuickActionsScrollViewer.ScrollableWidth;
        var offset = System.Math.Min(currentOffset + 256, maxOffset);
        QuickActionsScrollViewer.ChangeView(offset, null, null);
    }

    private void QuickActionsScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs args)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            ScrollLeftButton.Visibility = scrollViewer.HorizontalOffset > 0 
                ? Microsoft.UI.Xaml.Visibility.Visible 
                : Microsoft.UI.Xaml.Visibility.Collapsed;

            ScrollRightButton.Visibility = scrollViewer.HorizontalOffset < scrollViewer.ScrollableWidth 
                ? Microsoft.UI.Xaml.Visibility.Visible 
                : Microsoft.UI.Xaml.Visibility.Collapsed;
        }
    }
}
