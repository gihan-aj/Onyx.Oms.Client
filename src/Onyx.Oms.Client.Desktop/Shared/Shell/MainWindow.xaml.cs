using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Onyx.Oms.Client.Desktop.Shared.Services;

namespace Onyx.Oms.Client.Desktop.Shared.Shell;

public sealed partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;
    private readonly INavigationViewService _navigationViewService;
    private readonly IAuthenticationService _authenticationService;

    public MainWindow(INavigationService navigationService, INavigationViewService navigationViewService, IAuthenticationService authenticationService)
    {
        InitializeComponent();

        _navigationService = navigationService;
        _navigationViewService = navigationViewService;
        _authenticationService = authenticationService;

        // Setup Custom TitleBar
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        // Initialize Navigation
        _navigationService.Frame = ContentFrame;
        _navigationViewService.Initialize(NavView);

        // Setup Auth UI
        CheckLoginStatus();
        _authenticationService.AuthenticationChanged += OnAuthenticationChanged;
        LoginView.LoginRequested += OnLoginRequested;
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

    private void OnAuthenticationChanged(object? sender, bool isAuthenticated)
    {
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

            // Animate Transition
            AnimateOpacity(LoginView, 1, 0, () => LoginView.Visibility = Visibility.Collapsed);
            NavView.Visibility = Visibility.Visible;
            AnimateOpacity(NavView, 0, 1);
        }
        else
        {
            // Unauthenticated State
            SignInBtn.Visibility = Visibility.Collapsed; // Hide TitleBar button, use LoginView instead
            SignOutBtn.Visibility = Visibility.Collapsed;
            UserAvatar.Visibility = Visibility.Collapsed;
            
            // Disable Pane Toggle
            AppTitleBar.IsPaneToggleButtonVisible = false;

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
