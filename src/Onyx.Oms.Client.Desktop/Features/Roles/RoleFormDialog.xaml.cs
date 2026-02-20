using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Data;
using System;

namespace Onyx.Oms.Client.Desktop.Features.Roles;

// Simple converter to make parent nodes Three-State and leaf nodes Two-State
public partial class NullToTrueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value == null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

// Inverted Bool Converter if it doesn't exist globally (usually does but let's be safe)
public partial class InvertedBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b) return !b;
        return true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is bool b) return !b;
        return false;
    }
}

public sealed partial class RoleFormDialog : ContentDialog
{
    public RoleFormViewModel ViewModel { get; }

    public RoleFormDialog(RoleFormViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }

    private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        args.Cancel = true; // Prevent close by default

        // Basic validation
        if (string.IsNullOrWhiteSpace(ViewModel.Name))
        {
            ErrorText.Text = "Name is required.";
            ErrorText.Visibility = Visibility.Visible;
            deferral.Complete();
            return;
        }

        ErrorText.Visibility = Visibility.Collapsed;

        var success = await ViewModel.SaveAsync();
        
        if (success)
        {
            args.Cancel = false; // Allow close on success
        }

        deferral.Complete();
    }
}
