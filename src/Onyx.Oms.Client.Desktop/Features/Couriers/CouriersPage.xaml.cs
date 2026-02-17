using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

public sealed partial class CouriersPage : Page
{
    public CouriersViewModel ViewModel { get; }

    public CouriersPage()
    {
        InitializeComponent();
        
        // Resolve ViewModel from DI (Service Locator)
        ViewModel = App.Current.Services.GetService(typeof(CouriersViewModel)) as CouriersViewModel 
                    ?? throw new InvalidOperationException("CouriersViewModel not found");
        
        DataContext = ViewModel;
    }

    private void OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (ViewModel.SearchCommand.CanExecute(args.QueryText))
        {
            ViewModel.SearchCommand.Execute(args.QueryText);
        }
    }

    private void OnDataGridSorting(object sender, CommunityToolkit.WinUI.UI.Controls.DataGridColumnEventArgs e)
    {
        if (e.Column.Tag is not string sortColumn) return;

        // Toggle sort direction
        if (e.Column.SortDirection == null || e.Column.SortDirection == CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Descending)
        {
            e.Column.SortDirection = CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Ascending;
        }
        else
        {
            e.Column.SortDirection = CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Descending;
        }

        // Clear sort direction of other columns
        foreach (var column in ((CommunityToolkit.WinUI.UI.Controls.DataGrid)sender).Columns)
        {
            if (column != e.Column)
            {
                column.SortDirection = null;
            }
        }

        // Trigger sort in ViewModel
        var direction = e.Column.SortDirection == CommunityToolkit.WinUI.UI.Controls.DataGridSortDirection.Ascending ? "asc" : "desc";
        _ = ViewModel.Sort(sortColumn, direction);
    }
}
