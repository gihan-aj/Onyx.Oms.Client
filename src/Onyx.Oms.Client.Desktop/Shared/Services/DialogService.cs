using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class DialogService : IDialogService
{
    private XamlRoot? _xamlRoot;

    public void RegisterXamlRoot(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
        CurrentXamlRoot = xamlRoot;
    }

    public XamlRoot? CurrentXamlRoot { get; private set; }

    public async Task ShowMessageAsync(string title, string message)
    {
        if (_xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = CreateContent(message),
            CloseButtonText = "OK",
            XamlRoot = _xamlRoot,
            RequestedTheme = (_xamlRoot.Content as FrameworkElement)?.ActualTheme ?? ElementTheme.Default
        };

        await dialog.ShowAsync();
    }

    public async Task ShowErrorAsync(string title, string message)
    {
        await ShowMessageAsync(title, message);
    }

    public async Task ShowValidationErrorsAsync(string title, IEnumerable<string> errors)
    {
        if (_xamlRoot == null) return;

        var content = new StackPanel { Spacing = 8 };
        foreach (var error in errors)
        {
            content.Children.Add(CreateContent($"• {error}"));
        }

        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "OK",
            XamlRoot = _xamlRoot,
            RequestedTheme = (_xamlRoot.Content as FrameworkElement)?.ActualTheme ?? ElementTheme.Default
        };

        await dialog.ShowAsync();
    }

    public async Task<bool> ShowConfirmationAsync(string title, string message, string yesText = "Yes", string noText = "No")
    {
        if (_xamlRoot == null) return false;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = CreateContent(message),
            PrimaryButtonText = yesText,
            CloseButtonText = noText,
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = _xamlRoot,
            RequestedTheme = (_xamlRoot.Content as FrameworkElement)?.ActualTheme ?? ElementTheme.Default
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    private TextBlock CreateContent(string text)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            TextWrapping = TextWrapping.Wrap
        };
        
        // Apply Style if available, otherwise fallback
        if (Application.Current.Resources.TryGetValue("BodyTextBlockStyle", out var style) && style is Style bodyStyle)
        {
            textBlock.Style = bodyStyle;
        }

        return textBlock;
    }
}
