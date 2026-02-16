using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class NavigationViewService : INavigationViewService
{
    private readonly INavigationService _navigationService;
    private readonly IPageService _pageService;
    private NavigationView? _navigationView;

    public IList<object>? MenuItems => _navigationView?.MenuItems;

    public object? SettingsItem => _navigationView?.SettingsItem;

    public NavigationViewService(INavigationService navigationService, IPageService pageService)
    {
        _navigationService = navigationService;
        _pageService = pageService;
    }

    public void Initialize(NavigationView navigationView)
    {
        _navigationView = navigationView;
        _navigationView.ItemInvoked += OnItemInvoked;
        _navigationService.Navigated += OnNavigated;
    }

    public void UnregisterEvents()
    {
        if (_navigationView != null)
        {
            _navigationView.ItemInvoked -= OnItemInvoked;
            _navigationView = null;
        }
        _navigationService.Navigated -= OnNavigated;
    }

    public NavigationViewItem? GetSelectedItem(Type pageType)
    {
        if (_navigationView != null)
        {
            return GetSelectedItem(_navigationView.MenuItems, pageType) ?? GetSelectedItem(_navigationView.FooterMenuItems, pageType);
        }

        return null;
    }

    private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked)
        {
            _navigationService.NavigateTo(typeof(Onyx.Oms.Client.Desktop.Features.Settings.SettingsPage).FullName!);
        }
        else
        {
            var selectedItem = args.InvokedItemContainer as NavigationViewItem;
            if (selectedItem?.Tag is string pageKey)
            {
                _navigationService.NavigateTo(pageKey);
            }
        }
    }

    private void OnNavigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        if (_navigationView != null)
        {
             _navigationView.IsBackEnabled = _navigationService.CanGoBack;
            if (e.SourcePageType == typeof(Onyx.Oms.Client.Desktop.Features.Settings.SettingsPage))
            {
                // SettingsItem is not a NavigationViewItem, so we handle it separately
                _navigationView.SelectedItem = (NavigationViewItem)_navigationView.SettingsItem;
            }
            else if (e.SourcePageType != null)
            {
                var selectedItem = GetSelectedItem(e.SourcePageType);
                if (selectedItem != null)
                {
                    _navigationView.SelectedItem = selectedItem;
                }
            }
        }
    }

    private NavigationViewItem? GetSelectedItem(IEnumerable<object> menuItems, Type pageType)
    {
        foreach (var item in menuItems.OfType<NavigationViewItem>())
        {
            if (IsMenuItemForPageType(item, pageType))
            {
                return item;
            }

            var selectedChild = GetSelectedItem(item.MenuItems, pageType);
            if (selectedChild != null)
            {
                return selectedChild;
            }
        }

        return null;
    }

    private bool IsMenuItemForPageType(NavigationViewItem menuItem, Type sourcePageType)
    {
        if (menuItem.Tag is string pageKey)
        {
            return _pageService.GetPageType(pageKey) == sourcePageType;
        }

        return false;
    }
}
