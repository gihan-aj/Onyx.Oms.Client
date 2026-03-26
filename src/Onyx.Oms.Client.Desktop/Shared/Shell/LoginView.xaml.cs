using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Onyx.Oms.Client.Desktop.Shared.Shell;

public sealed partial class LoginView : UserControl
{
    public event EventHandler? LoginRequested;

    public LoginView()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        LoginRequested?.Invoke(this, EventArgs.Empty);
    }

    public void SetLoading(bool isLoading)
    {
        if (isLoading)
        {
            LoginButton.Visibility = Visibility.Collapsed;
            LoginText.Visibility = Visibility.Collapsed;
            LoadingText.Visibility = Visibility.Visible;
            LoadingSpinner.Visibility = Visibility.Visible;
            LoadingSpinner.IsActive = true;
        }
        else
        {
            LoadingText.Visibility = Visibility.Collapsed;
            LoadingSpinner.Visibility = Visibility.Collapsed;
            LoadingSpinner.IsActive = false;
        }
    }

    public void UpdateLoginButtonVisibility(bool isAuthenticated)
    {
        LoginText.Visibility = isAuthenticated ? Visibility.Collapsed : Visibility.Visible;
        LoginButton.Visibility = isAuthenticated ? Visibility.Collapsed : Visibility.Visible;
    }
}
