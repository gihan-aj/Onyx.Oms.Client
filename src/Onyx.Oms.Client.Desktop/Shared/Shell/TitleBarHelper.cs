using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using System.Runtime.InteropServices;
using WinRT.Interop;
using Microsoft.UI.Xaml.Media;
using System;

namespace Onyx.Oms.Client.Desktop.Shared.Shell;

public class TitleBarHelper
{
    private readonly Window _window;
    private readonly TitleBar _titleBar;

    public TitleBarHelper(Window window, TitleBar titleBar)
    {
        _window = window;
        _titleBar = titleBar;
    }

    public void ApplySystemThemeToCaptionButtons()
    {
        var appWindow = GetAppWindow(_window);
        if (appWindow != null)
        {
             // Check theme and apply color
             // For now, let's just ensure standard behavior or transparent background
             // Accessing AppWindow.TitleBar...
             var titleBar = appWindow.TitleBar;
             titleBar.ExtendsContentIntoTitleBar = true;
             
             // Define draggable regions if needed, but the XAML TitleBar element handles this when set via Window.SetTitleBar
        }
    }

    private AppWindow GetAppWindow(Window window)
    {
        IntPtr windowHandle = WindowNative.GetWindowHandle(window);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
        return AppWindow.GetFromWindowId(windowId);
    }
}
