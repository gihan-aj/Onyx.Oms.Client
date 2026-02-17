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
}
