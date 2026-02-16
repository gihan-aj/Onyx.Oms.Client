# TitleBar Implementation Guide

The WinUI Gallery uses the standard `TitleBar` control introduced in **Windows App SDK 1.6**. This control comes with built-in functionality for the Back button and Pane Toggle button, making implementation very straightforward.

## Prerequisite
Ensure your project is referencing **Windows App SDK 1.6** or later.
Check your `.csproj` file:
```xml
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.6.231115001" /> <!-- Or newer -->
```

## 1. XAML Implementation
In your `MainWindow.xaml`, add the `<TitleBar>` control. You don't need a custom control; it's available in the default namespace.

```xml
<Window
    ...
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" /> <!-- TitleBar Row -->
            <RowDefinition Height="*" />    <!-- Content Row -->
        </Grid.RowDefinitions>

        <TitleBar
            x:Name="AppTitleBar"
            Title="My App Name"
            Grid.Row="0"
            IsBackButtonVisible="{x:Bind ContentFrame.CanGoBack, Mode=OneWay}"
            IsPaneToggleButtonVisible="True"
            BackRequested="AppTitleBar_BackRequested"
            PaneToggleRequested="AppTitleBar_PaneToggleRequested">
            
            <!-- Optional: Icon -->
            <TitleBar.IconSource>
                <ImageIconSource ImageSource="/Assets/WindowIcon.ico" />
            </TitleBar.IconSource>
            
            <!-- Optional: Search Box or other content -->
            <TitleBar.Content>
                <AutoSuggestBox Width="300" VerticalAlignment="Center" />
            </TitleBar.Content>
        </TitleBar>

        <!-- Your Navigation View -->
        <NavigationView x:Name="NavView" Grid.Row="1" ...>
            <Frame x:Name="ContentFrame" ... />
        </NavigationView>
    </Grid>
</Window>
```

## 2. C# Code Behind
In `MainWindow.xaml.cs`, you need to:
1.  Verify the binding or manually update the Back button visibility.
2.  Handle the `BackRequested` event.
3.  Handle the `PaneToggleRequested` event.

```csharp
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        
        // 1. Tell the Window to use our custom XAML element
        this.ExtendsContentIntoTitleBar = true;
        this.SetTitleBar(AppTitleBar); // passing the XAML element defined above
    }

    // 2. Handle Back Request
    private void AppTitleBar_BackRequested(TitleBar sender, object args)
    {
        if (ContentFrame.CanGoBack)
        {
            ContentFrame.GoBack();
        }
    }

    // 3. Handle Pane Toggle Request
    private void AppTitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        // Toggle the Navigation View Pane
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    // Optional: Ensure Back button state is updated if you are not using x:Bind OneWay
    // (If using x:Bind to Frame.CanGoBack, this might be handled automatically if Frame notifies changes, 
    //  but Frame.CanGoBack is not a DependencyProperty, so you might need to listen to Navigated event)
    private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        // If x:Bind doesn't update automatically (CanGoBack isn't observable), update manually:
        // AppTitleBar.IsBackButtonVisible = ContentFrame.CanGoBack;
    }
}
```

## Key Points
*   **`IsPaneToggleButtonVisible="True"`**: This single property enables the hamburger menu button in the TitleBar.
*   **`PaneToggleRequested`**: This event fires when the button is clicked. You simply toggle your `NavigationView.IsPaneOpen`.
*   **`SetTitleBar(AppTitleBar)`**: This is critical. It tells the OS that this XAML element is serving as the window's title bar, effectively removing the system default one but keeping caption buttons (Close, Maximize, Minimize).

## 3. Advanced: User Profile & Actions (Right Aligned)
To add a profile photo, Sign In, and Sign Out buttons aligned to the right (next to the caption buttons), use the `Footer` property (or `RightHeader` in some preview versions).

```xml
<TitleBar x:Name="AppTitleBar" ...>
    <!-- ... Icon and Content ... -->

    <!-- Footer / RightHeader: Content displayed on the right side -->
    <TitleBar.Footer>
        <StackPanel Orientation="Horizontal" Spacing="8" Margin="0,0,12,0">
             <!-- Sign In / Sign Out Buttons -->
            <Button x:Name="SignInBtn" Content="Sign In" Click="SignInBtn_Click" Style="{StaticResource AccentButtonStyle}" />
            <Button x:Name="SignOutBtn" Content="Sign Out" Click="SignOutBtn_Click" Visibility="Collapsed" />
            
            <!-- User Profile Picture -->
            <PersonPicture 
                x:Name="UserAvatar" 
                Width="32" 
                DisplayName="John Doe" 
                Initials="JD" 
                Visibility="Collapsed" />
        </StackPanel>
    </TitleBar.Footer>
</TitleBar>
```
**Note:** If `Footer` works, use it. If you are using an older/experimental version of Windows App SDK (like the one in this Gallery repo), the property might be named `RightHeader`. titlebar properties have changed names between experimental and stable releases.

### C# Logic
Handle the visibility toggle in your code-behind:

```csharp
private void SignInBtn_Click(object sender, RoutedEventArgs e)
{
    // Mock Sign In
    SignInBtn.Visibility = Visibility.Collapsed;
    SignOutBtn.Visibility = Visibility.Visible;
    UserAvatar.Visibility = Visibility.Visible;
}

private void SignOutBtn_Click(object sender, RoutedEventArgs e)
{
    // Mock Sign Out
    SignInBtn.Visibility = Visibility.Visible;
    SignOutBtn.Visibility = Visibility.Collapsed;
    UserAvatar.Visibility = Visibility.Collapsed;
}
```
