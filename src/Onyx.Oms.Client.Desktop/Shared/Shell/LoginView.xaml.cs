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
}
