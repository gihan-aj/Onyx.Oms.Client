using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System.Linq;

namespace Onyx.Oms.Client.Desktop.Shared.Shell;

public sealed partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;
    private readonly INavigationViewService _navigationViewService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IPermissionService _permissionService;

    private readonly IDialogService _dialogService;
    private readonly IToastService _toastService;

    public MainWindow(
        INavigationService navigationService, 
        INavigationViewService navigationViewService, 
        IAuthenticationService authenticationService,
        IPermissionService permissionService,
        IDialogService dialogService,
        IToastService toastService)
    {
        InitializeComponent();

        _navigationService = navigationService;
        _navigationViewService = navigationViewService;
        _authenticationService = authenticationService;
        _permissionService = permissionService;
        _dialogService = dialogService;
        _toastService = toastService;

        // Setup Custom TitleBar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        // Initialize Services
        _navigationService.Frame = ContentFrame;
        _navigationViewService.Initialize(NavView);
        
        // Register InfoBar (Can be done here as object exists)
        if (_toastService is ToastService ts) ts.RegisterInfoBar(ShellInfoBar);

        // Register XamlRoot when loaded (Required for ContentDialog)
        RootGrid.Loaded += (s, e) =>
        {
            if (_dialogService is DialogService ds) ds.RegisterXamlRoot(RootGrid.XamlRoot);
        };

        // Setup Auth UI
        CheckLoginStatus();
        _authenticationService.AuthenticationChanged += OnAuthenticationChanged;
        LoginView.LoginRequested += OnLoginRequested;

        // Ensure app shuts down when window is closed
        Closed += (s, e) => 
        {
             // Unsubscribe to prevent memory leak (Singleton holding ref to MainWindow)
             _authenticationService.AuthenticationChanged -= OnAuthenticationChanged;
             LoginView.LoginRequested -= OnLoginRequested;

             // Make sure to dispose settings or services if needed
             Microsoft.UI.Xaml.Application.Current.Exit();
        };
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        // Handled by NavigationViewService
    }

    private void AppTitleBar_BackRequested(TitleBar sender, object args)
    {
        if(ContentFrame.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }

    private void AppTitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    private async void OnAuthenticationChanged(object? sender, bool isAuthenticated)
    {
        if (isAuthenticated)
        {
            // Block UI update until permissions are fetched.
            // The user will see exactly what they saw during login (the splash/loading state)
            await _permissionService.InitializeAsync();
        }
        else
        {
            _permissionService.ClearPermissions();
        }

        // UI updates must happen on the UI thread
        DispatcherQueue.TryEnqueue(() =>
        {
            UpdateAuthenticationUI();
        });
    }

    private async void OnLoginRequested(object? sender, EventArgs e)
    {
         await _authenticationService.LoginAsync();
    }

    private void CheckLoginStatus()
    {
        // Initial check
        UpdateAuthenticationUI();
    }

    private void UpdateAuthenticationUI()
    {
        if (_authenticationService.IsAuthenticated)
        {
            // Authenticated State
            LoginView.Visibility = Visibility.Collapsed;
            NavView.Visibility = Visibility.Visible;
            
            SignInBtn.Visibility = Visibility.Collapsed;
            SignOutBtn.Visibility = Visibility.Visible;
            UserAvatar.Visibility = Visibility.Visible;
            
            UserAvatar.DisplayName = _authenticationService.User.Identity?.Name ?? "User";
            
            var givenName = _authenticationService.User.FindFirst("given_name")?.Value ?? "";
            var familyName = _authenticationService.User.FindFirst("family_name")?.Value ?? "";
            var initials = "";
            if (givenName.Length > 0) initials += givenName[0];
            if (familyName.Length > 0) initials += familyName[0];
            UserAvatar.Initials = initials;
            
            // Enable Pane Toggle
            AppTitleBar.IsPaneToggleButtonVisible = true;

            // Filter Navigation Menu Items based on Permissions
            foreach (var item in NavView.MenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag is string pageKey)
                {
                    navItem.Visibility = _permissionService.CanNavigateTo(pageKey) ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            // Animate Transition
            AnimateOpacity(LoginView, 1, 0, () => LoginView.Visibility = Visibility.Collapsed);
            NavView.Visibility = Visibility.Visible;
            AnimateOpacity(NavView, 0, 1);

            // Navigate to default page on fresh login
            if (ContentFrame.Content == null)
            {
                var dashboardKey = typeof(Features.Dashboard.DashboardPage).FullName!;
                _navigationService.NavigateTo(dashboardKey, null, true);
                
                // Select dashboard item in the menu
                var dashboardItem = System.Linq.Enumerable.OfType<NavigationViewItem>(NavView.MenuItems)
                    .FirstOrDefault(i => (string)i.Tag == dashboardKey);
                if (dashboardItem != null)
                {
                    NavView.SelectedItem = dashboardItem;
                }
            }
        }
        else
        {
            // Unauthenticated State
            SignInBtn.Visibility = Visibility.Collapsed; // Hide TitleBar button, use LoginView instead
            SignOutBtn.Visibility = Visibility.Collapsed;
            UserAvatar.Visibility = Visibility.Collapsed;
            
            // Disable Pane Toggle
            AppTitleBar.IsPaneToggleButtonVisible = false;

            // Clear frame content and history so previous user's data isn't preserved
            ContentFrame.Content = null;
            ContentFrame.BackStack.Clear();
            ContentFrame.ForwardStack.Clear();

            // Animate Transition
            LoginView.Visibility = Visibility.Visible;
            AnimateOpacity(LoginView, 0, 1);
            AnimateOpacity(NavView, 1, 0, () => NavView.Visibility = Visibility.Collapsed);
        }
    }

    private void AnimateOpacity(UIElement element, double from, double to, Action? onCompleted = null)
    {
        var storyboard = new Microsoft.UI.Xaml.Media.Animation.Storyboard();
        var animation = new Microsoft.UI.Xaml.Media.Animation.DoubleAnimation
        {
            From = from,
            To = to,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new Microsoft.UI.Xaml.Media.Animation.CubicEase { EasingMode = Microsoft.UI.Xaml.Media.Animation.EasingMode.EaseInOut }
        };
        
        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTarget(animation, element);
        Microsoft.UI.Xaml.Media.Animation.Storyboard.SetTargetProperty(animation, "Opacity");
        
        storyboard.Children.Add(animation);
        
        if (onCompleted != null)
        {
            storyboard.Completed += (s, e) => onCompleted();
        }
        
        storyboard.Begin();
    }

    private async void SignInBtn_Click(object sender, RoutedEventArgs e)
    {
        await _authenticationService.LoginAsync();
    }

    private async void SignOutBtn_Click(object sender, RoutedEventArgs e)
    {
        await _authenticationService.LogoutAsync();
    }
}
