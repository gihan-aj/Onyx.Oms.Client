# Project Setup & Architecture

This guide outlines how to set up your new WinUI 3 project with a Vertical Slice Architecture (VSA), incorporating the shell elements from the WinUI Gallery.

## 1. Solution Structure (VSA)
We recommend the following folder structure for your Order Management System:

```
src/
  MyOrderApp/                  (Main Project)
    Features/                  (Vertical Slices)
      Orders/
        Components/
        Models/
        Services/
        Views/
      Inventory/
      Customers/
    Shared/                    (Shared components)
      Shell/                   (The extracted Shell goes here)
        MainWindow.xaml
        MainWindow.xaml.cs
        Navigation/
      Services/                (Core infrastructure)
        NavigationService.cs
        ThemeService.cs
      Components/              (Reusable UI components)
    App.xaml
    App.xaml.cs
```

## 2. Initializing the Project
1.  Open Visual Studio.
2.  Create a new **Blank App, Packaged (WinUI 3 in Desktop)** project.
3.  Name it `MyOrderApp`.
4.  Update the `Microsoft.WindowsAppSDK` NuGet package to the latest stable version (Ensure it is **1.6+** to use the `TitleBar` control).
5.  Install the required packages listed in [packages.md](packages.md).

## 3. Configuring the Shell
You will replace the default `MainWindow` with the enhanced shell extracted from WinUI Gallery. The core shell consists of:
- **Navigation System**: `NavigationView` for top-level navigation.
- **Modern TitleBar**: Using the integrated `TitleBar` control.
- **Backdrop**: `Mica` material for a modern feel.

See [shell_ui.md](shell_ui.md) for the implementation details.
