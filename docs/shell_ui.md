# Shell UI Implementation

This guide explains how to implement the main shell of the application, including the Navigation View and the custom Title Bar.

## 1. App.xaml Components
Ensure your `App.xaml` merges necessary resources. You may want to copy the `Styles` folder from the WinUI Gallery if you want the exact same typography and control styles.

```xml
<!-- App.xaml -->
<Application
    ...
    xmlns:local="using:MyOrderApp">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
                <!-- Add other style dictionaries here if you copied them -->
                <!-- <ResourceDictionary Source="Styles/Grid.xaml" /> -->
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

## 2. MainWindow.xaml
This is the core shell. It uses a `NavigationView` to host the main content `Frame`.

```xml
<!-- MainWindow.xaml -->
<Window
    x:Class="MyOrderApp.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:local="using:MyOrderApp"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d">

    <Window.SystemBackdrop>
        <MicaBackdrop />
    </Window.SystemBackdrop>

    <Grid x:Name="RootGrid">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
        </Grid.RowDefinitions>

        <!-- Custom Title Bar -->
        <TitleBar
            x:Name="AppTitleBar"
            Title="My Order App"
            Grid.Row="0">
            <!-- Optional: Add a search box or other content here -->
            <TitleBar.Content>
                <AutoSuggestBox 
                    x:Name="SearchBox" 
                    Width="300" 
                    PlaceholderText="Search orders..." 
                    VerticalAlignment="Center" />
            </TitleBar.Content>
        </TitleBar>

        <!-- Navigation View -->
        <NavigationView
            x:Name="NavView"
            Grid.Row="1"
            IsBackButtonVisible="Collapsed"
            PaneDisplayMode="Auto"
            SelectionChanged="NavView_SelectionChanged"
            Loaded="NavView_Loaded">
            
            <NavigationView.MenuItems>
                <NavigationViewItem Icon="Home" Content="Dashboard" Tag="DashboardPage" />
                <NavigationViewItem Icon="Shop" Content="Orders" Tag="OrdersPage" />
                <NavigationViewItem Icon="Contact" Content="Customers" Tag="CustomersPage" />
            </NavigationView.MenuItems>
            
            <NavigationView.FooterMenuItems>
                <NavigationViewItem Icon="Settings" Content="Settings" Tag="SettingsPage" />
            </NavigationView.FooterMenuItems>

            <Frame x:Name="ContentFrame" />
        </NavigationView>
    </Grid>
</Window>
```

## 3. MainWindow.xaml.cs
Handles the navigation logic and integrates the services.

```csharp
// MainWindow.xaml.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyOrderApp.Services;
using System;

namespace MyOrderApp;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();

        // Setup Custom TitleBar
        this.ExtendsContentIntoTitleBar = true;
        this.SetTitleBar(AppTitleBar); // Using the Element defined in XAML

        // Apply theme to caption buttons (minimize/close)
        TitleBarHelper.ApplySystemThemeToCaptionButtons(this, ThemeHelper.ActualTheme);
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        // Navigate to the first page by default
        NavView.SelectedItem = NavView.MenuItems[0];
        NavView_Navigate("DashboardPage", new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            NavView_Navigate("SettingsPage", args.RecommendedNavigationTransitionInfo);
        }
        else if (args.SelectedItemContainer != null)
        {
            var navItemTag = args.SelectedItemContainer.Tag.ToString();
            NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);
        }
    }

    private void NavView_Navigate(string navItemTag, Microsoft.UI.Xaml.Media.Animation.NavigationTransitionInfo transitionInfo)
    {
        Type _page = null;

        switch (navItemTag)
        {
            case "DashboardPage":
                _page = typeof(Views.DashboardPage); // Use your actual Page types
                break;
            case "OrdersPage":
                 // _page = typeof(Views.OrdersPage);
                break;
            case "CustomersPage":
                 // _page = typeof(Views.CustomersPage);
                break;
            case "SettingsPage":
                 // _page = typeof(Views.SettingsPage);
                break;
        }

        // Get the page type before navigation so you can prevent duplicate entries in the backstack
        var preNavPageType = ContentFrame.CurrentSourcePageType;

        // Only navigate if the selected page isn't currently loaded
        if (!(_page is null) && !Type.Equals(preNavPageType, _page))
        {
            ContentFrame.Navigate(_page, null, transitionInfo);
        }
    }
}
```

## 4. Shell Styling
To match the "Modern" look of the Gallery:
1.  **Backdrop**: Ensure `<MicaBackdrop />` is used.
2.  **Icons**: Use `FontIcon` or `SymbolIcon` in `NavigationViewItem`.
3.  **Styles**: Copy `Styles/Grid.xaml` and `Styles/TextBlock.xaml` from the Gallery explicitly if you want the custom grid and text styles.
