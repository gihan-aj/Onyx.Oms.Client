using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

public sealed partial class CouriersPage : Page
{
    private readonly IToastService _toastService;
    private readonly IDialogService _dialogService;

    public CouriersPage()
    {
        InitializeComponent();
        
        // Resolve services using Service Locator pattern since Frame doesn't support DI constructor injection
        _toastService = App.Current.Services.GetService(typeof(IToastService)) as IToastService ?? throw new InvalidOperationException("IToastService not found");
        _dialogService = App.Current.Services.GetService(typeof(IDialogService)) as IDialogService ?? throw new InvalidOperationException("IDialogService not found");
    }

    private void OnSuccessToastClick(object sender, RoutedEventArgs e)
    {
        _toastService.ShowSuccess("Success", "This is a success message that will disappear in 3 seconds.");
    }

    private void OnErrorToastClick(object sender, RoutedEventArgs e)
    {
        _toastService.ShowError("Error", "Something went wrong! This is an error message.");
    }

    private async void OnConfirmationDialogClick(object sender, RoutedEventArgs e)
    {
        var result = await _dialogService.ShowConfirmationAsync("Delete Courier?", "Are you sure you want to delete this courier? This action cannot be undone.");
        if (result)
        {
            _toastService.ShowInfo("Confirmed", "You clicked Yes.");
        }
        else
        {
            _toastService.ShowInfo("Cancelled", "You clicked No.");
        }
    }

    private async void OnValidationDialogClick(object sender, RoutedEventArgs e)
    {
        var errors = new List<string>
        {
            "Courier Name is required.",
            "Primary Phone is invalid.",
            "Website URL must be a valid HTTPS URL."
        };
        await _dialogService.ShowValidationErrorsAsync("Validation Failed", errors);
    }
}
