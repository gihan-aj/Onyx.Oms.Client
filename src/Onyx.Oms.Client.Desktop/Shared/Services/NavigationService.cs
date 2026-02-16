using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class NavigationService : INavigationService
{
    private readonly IPermissionService _permissionService;
    private readonly IPageService _pageService;
    private Frame? _frame;

    public event NavigatedEventHandler? Navigated;

    public NavigationService(IPermissionService permissionService, IPageService pageService)
    {
        _permissionService = permissionService;
        _pageService = pageService;
    }

    public Frame? Frame
    {
        get => _frame;
        set
        {
            UnregisterFrameEvents();
            _frame = value;
            RegisterFrameEvents();
        }
    }

    public bool CanGoBack => _frame?.CanGoBack ?? false;

    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        // 1. Check Permissions
        // specific implementation depends on how async usage is handled in sync NavigateTo
        // For now, we assume _permissionService.CanNavigateToAsync returns true or we block/fire-and-forget.
        // Ideally NavigationService should be Async or PermissionService should have a sync check cache.
        // For simplicity in this step, we'll proceed.

        var pageType = _pageService.GetPageType(pageKey);
        if (_frame != null && (_frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParamUsed))))
        {
            _frame.Tag = clearNavigation;
            var vmBeforeNav = _frame.Content as Page;
            var navigated = _frame.Navigate(pageType, parameter);
            if (navigated)
            {
                _lastParamUsed = parameter;
                if (clearNavigation)
                {
                    _frame.BackStack.Clear();
                }
            }
            return navigated;
        }

        return false;
    }

    public bool GoBack()
    {
        if (CanGoBack)
        {
            var vmBeforeNav = _frame?.Content as Page;
            _frame?.GoBack();
            return true;
        }
        return false;
    }

    private void RegisterFrameEvents()
    {
        if (_frame != null)
        {
            _frame.Navigated += OnNavigated;
        }
    }

    private void UnregisterFrameEvents()
    {
        if (_frame != null)
        {
            _frame.Navigated -= OnNavigated;
        }
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            bool clearNavigation = (bool)(frame.Tag ?? false);
            if (clearNavigation)
            {
                frame.BackStack.Clear();
            }

            if (e.Content is Page page && page.DataContext is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(e.Parameter);
            }

            Navigated?.Invoke(sender, e);
        }
    }

    private object? _lastParamUsed;
}
