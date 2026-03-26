using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Onyx.Oms.Client.Desktop.Shared.Shell;

public sealed partial class LoginView : UserControl
{
    public event EventHandler? LoginRequested;
    public event EventHandler? RegisterRequested;

    public LoginView()
    {
        InitializeComponent();
        LoadingText.Visibility = Visibility.Collapsed;
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorMessageText.Visibility = Visibility.Collapsed;
        LoginRequested?.Invoke(this, EventArgs.Empty);
    }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorMessageText.Visibility = Visibility.Collapsed;
        RegisterRequested?.Invoke(this, EventArgs.Empty);
    }

    public void SetLoading(bool isLoading)
    {
        if (isLoading)
        {
            LoginButton.Visibility = Visibility.Collapsed;
            RegisterButton.Visibility = Visibility.Collapsed;
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
        RegisterButton.Visibility = isAuthenticated ? Visibility.Collapsed : Visibility.Visible;
    }

    public void ShowError(string message)
    {
        ErrorMessageText.Text = message;
        ErrorMessageText.Visibility = Visibility.Visible;
    }
}
