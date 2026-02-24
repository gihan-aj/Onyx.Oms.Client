using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;

namespace Onyx.Oms.Client.Desktop.Features.Roles;

public sealed partial class RolesPage : Page
{
    public RolesViewModel ViewModel { get; }

    public RolesPage()
    {
        ViewModel = App.Current.Services.GetRequiredService(typeof(RolesViewModel)) as RolesViewModel
            ?? throw new InvalidOperationException("RolesViewModel not found"); ;
        InitializeComponent();

        DataContext = ViewModel;
    }

    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (ViewModel.SearchCommand.CanExecute(sender.Text))
        {
            ViewModel.SearchCommand.Execute(sender.Text);
        }
    }

    private async void OnDataGridSorting(object sender, DataGridColumnEventArgs e)
    {
        var tag = e.Column.Tag?.ToString();
        if (string.IsNullOrEmpty(tag)) return;

        // Determine sort direction
        string newDirection = "asc";
        if (e.Column.SortDirection == null)
        {
            newDirection = "asc";
        }
        else if (e.Column.SortDirection == DataGridSortDirection.Ascending)
        {
            newDirection = "desc";
        }
        else if (e.Column.SortDirection == DataGridSortDirection.Descending)
        {
            newDirection = "asc"; // Depending on desired behavior, could reset to null
        }

        // Reset all columns
        foreach (var column in ((DataGrid)sender).Columns)
        {
            column.SortDirection = null;
        }

        // Set visual indicator on clicked column
        e.Column.SortDirection = newDirection == "asc" ? DataGridSortDirection.Ascending : DataGridSortDirection.Descending;

        await ViewModel.Sort(tag, newDirection);
    }

    private async void OnViewClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is RoleDto role)
        {
            try
            {
                ViewModel.IsBusy = true;
                
                var details = await ViewModel.GetRoleDetailsAsync(role.Id);
                if (details == null) return;
                
                var vm = App.Current.Services.GetRequiredService<RoleFormViewModel>();
                await vm.InitializeAsync(details, isReadOnly: true);

                var dialog = new RoleFormDialog(vm)
                {
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            }
            finally
            {
                ViewModel.IsBusy = false;
            }
        }
    }

    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is RoleDto role)
        {
            if (ViewModel.EditRoleCommand.CanExecute(role))
            {
                ViewModel.EditRoleCommand.Execute(role);
            }
        }
    }

    private void OnDeleteClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is RoleDto role)
        {
            if (ViewModel.DeleteCommand.CanExecute(role))
            {
                ViewModel.DeleteCommand.Execute(role);
            }
        }
    }

    private void OnActivateClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is RoleDto role)
        {
            if (ViewModel.ActivateCommand.CanExecute(role))
            {
                ViewModel.ActivateCommand.Execute(role);
            }
        }
    }

    private void OnDeactivateClick(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is RoleDto role)
        {
            if (ViewModel.DeactivateCommand.CanExecute(role))
            {
                ViewModel.DeactivateCommand.Execute(role);
            }
        }
    }
}
