using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using Microsoft.Extensions.DependencyInjection;
using Onyx.Oms.Client.Desktop.Shared.Constants;
using Onyx.Oms.Client.Desktop.Shared.Services;

namespace Onyx.Oms.Client.Desktop.Features.ProductCategories;

public sealed partial class ProductCategoriesPage : Page
{
    public ProductCategoriesViewModel ViewModel { get; }

    public ProductCategoriesPage()
    {
        InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ProductCategoriesViewModel>();

        DataContext = ViewModel;

        // Page-level Action Permissions
        var permissionService = App.Current.Services.GetRequiredService<IPermissionService>();
        NewRootCategoryButton.Visibility = permissionService.CanExecute(Permissions.ProductCategories.Create)
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        ViewModel.RefreshCommand.Execute(null);
    }

    private void OnNewRootCategoryClick(object sender, RoutedEventArgs e)
    {
        var navService = App.Current.Services.GetRequiredService<INavigationService>();
        navService.NavigateTo(typeof(ProductCategoryFormViewModel).FullName!, new ProductCategoryFormNavArgs());
    }

    private void OnEditCategoryClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCategory != null)
        {
            var navService = App.Current.Services.GetRequiredService<INavigationService>();
            navService.NavigateTo(typeof(ProductCategoryFormViewModel).FullName!, new ProductCategoryFormNavArgs { CategoryId = ViewModel.SelectedCategory.Id });
        }
    }

    private void OnAddSubcategoryClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCategory != null)
        {
            var navService = App.Current.Services.GetRequiredService<INavigationService>();
            navService.NavigateTo(typeof(ProductCategoryFormViewModel).FullName!, new ProductCategoryFormNavArgs { PreselectedParentId = ViewModel.SelectedCategory.Id });
        }
    }

    private void OnActivateCategoryClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCategory != null)
        {
            ViewModel.ActivateCommand.Execute(ViewModel.SelectedCategory);
        }
    }

    private void OnDeactivateCategoryClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCategory != null)
        {
            ViewModel.DeactivateCommand.Execute(ViewModel.SelectedCategory);
        }
    }

    private void OnDeleteCategoryClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SelectedCategory != null)
        {
            ViewModel.DeleteCommand.Execute(ViewModel.SelectedCategory);
        }
    }

    private void OnNewRootClick(object sender, RoutedEventArgs e)
    {

    }
}
