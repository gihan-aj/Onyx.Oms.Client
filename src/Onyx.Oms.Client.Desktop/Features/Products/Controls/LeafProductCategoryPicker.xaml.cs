using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Onyx.Oms.Client.Desktop.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products.Controls;

public sealed partial class LeafProductCategoryPicker : UserControl
{
    private CancellationTokenSource? _searchCts;
    private ScrollViewer? _listScrollViewer;
    private bool _isLoadingNextPage;

    public LeafProductCategoryPicker()
    {
        InitializeComponent();
        Items = new ObservableCollection<ProductCategoryDto>();
    }

    public ObservableCollection<ProductCategoryDto> Items
    {
        get => (ObservableCollection<ProductCategoryDto>)GetValue(ItemsProperty);
        private set => SetValue(ItemsProperty, value);
    }
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<ProductCategoryDto>), typeof(LeafProductCategoryPicker), new PropertyMetadata(null));

    public ProductCategoryDto? SelectedCategory
    {
        get => (ProductCategoryDto?)GetValue(SelectedCategoryProperty);
        set => SetValue(SelectedCategoryProperty, value);
    }
    public static readonly DependencyProperty SelectedCategoryProperty =
        DependencyProperty.Register(nameof(SelectedCategory), typeof(ProductCategoryDto), typeof(LeafProductCategoryPicker), new PropertyMetadata(null, OnSelectedCategoryChanged));

    private static void OnSelectedCategoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (LeafProductCategoryPicker)d;
        picker.UpdateDisplayMember();
    }

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }
    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(LeafProductCategoryPicker), new PropertyMetadata("Select category", OnPlaceholderTextChanged));

    private static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var picker = (LeafProductCategoryPicker)d;
        if (picker.SelectedCategory == null)
        {
            picker.HeaderButtonText.Text = (string)e.NewValue;
        }
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(LeafProductCategoryPicker), new PropertyMetadata(false));

    public Func<string, int, int, CancellationToken, Task<PagedResult<ProductCategoryDto>>>? FetchDataDelegate { get; set; }

    private int _currentPage = 1;
    private const int PageSize = 10;
    private bool _hasMore = true;
    private string _currentSearchTerm = string.Empty;

    private void PickerButton_Click(object sender, RoutedEventArgs e)
    {
        PickerButton.Flyout?.ShowAt(PickerButton);
    }

    private async void Flyout_Opened(object sender, object e)
    {
        // Make the flyout content width match the button width
        FlyoutRootGrid.Width = PickerButton.ActualWidth;

        SearchBox.Text = string.Empty;
        SearchBox.Focus(FocusState.Programmatic);
        if (Items.Count == 0)
        {
            await LoadPageAsync(1, string.Empty);
        }
    }

    private void Flyout_Closed(object sender, object e)
    {
    }

    private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        var term = SearchBox.Text;

        try
        {
            await Task.Delay(300, token); // Debounce
            if (!token.IsCancellationRequested)
            {
                await LoadPageAsync(1, term);
            }
        }
        catch (TaskCanceledException) { }
    }

    private async Task LoadPageAsync(int page, string searchTerm, CancellationToken token = default)
    {
        if (FetchDataDelegate == null) return;

        IsLoading = true;
        _isLoadingNextPage = page > 1;

        try
        {
            var result = await FetchDataDelegate(searchTerm, page, PageSize, token);
            if (page == 1)
            {
                Items.Clear();
                _currentPage = 1;
                _currentSearchTerm = searchTerm;

                _listScrollViewer?.ChangeView(null, 0, null);
            }
            _hasMore = result.HasNextPage;
            _currentPage = result.Page;

            foreach (var item in result.Items)
            {
                Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            _isLoadingNextPage = false;
        }
    }

    private void ItemsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ItemsListView.SelectedItem is ProductCategoryDto selected)
        {
            SelectedCategory = selected;
            PickerButton.Flyout?.Hide();
            SearchBox.Text = string.Empty;
            ItemsListView.SelectedItem = null;
        }
    }

    private void UpdateDisplayMember()
    {
        if (SelectedCategory == null)
        {
            HeaderButtonText.Text = PlaceholderText;
            return;
        }

        HeaderButtonText.Text = !string.IsNullOrEmpty(SelectedCategory.NamePath) 
            ? SelectedCategory.NamePath 
            : SelectedCategory.Name;
    }

    private void ItemsListView_Loaded(object sender, RoutedEventArgs e)
    {
        _listScrollViewer = FindVisualChild<ScrollViewer>(ItemsListView);
        if (_listScrollViewer != null)
        {
            _listScrollViewer.ViewChanged += ListScrollViewer_ViewChanged;
        }
    }

    private void ItemsListView_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_listScrollViewer != null)
        {
            _listScrollViewer.ViewChanged -= ListScrollViewer_ViewChanged;
            _listScrollViewer = null;
        }
    }

    private async void ListScrollViewer_ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
    {
        if (_listScrollViewer == null || _isLoadingNextPage || IsLoading || !_hasMore)
            return;

        if (_listScrollViewer.VerticalOffset >= _listScrollViewer.ScrollableHeight - 50)
        {
            await LoadPageAsync(_currentPage + 1, _currentSearchTerm);
        }
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T found)
            {
                return found;
            }
            else
            {
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
        }
        return null;
    }

    public Visibility HasItemsToHideRing(int count)
    {
        return count == 0 && IsLoading ? Visibility.Visible : Visibility.Collapsed;
    }

    public Visibility IsLoadingNextPageVisibility(bool isLoading, int count)
    {
        return count > 0 && isLoading ? Visibility.Visible : Visibility.Collapsed;
    }
}
