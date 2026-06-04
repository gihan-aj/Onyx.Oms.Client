using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Onyx.Oms.Client.Desktop.Shared.Shell;

public sealed partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;
    private readonly INavigationViewService _navigationViewService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IPermissionService _permissionService;
    private readonly ITenantProfileService _tenantProfileService;
    private readonly ILicenseManagerService _licenseManagerService;
    private readonly DatabaseRestoreService _databaseRestoreService;

    private readonly IDialogService _dialogService;
    private readonly IToastService _toastService;

    private readonly BackgroundProcessService _backgroundServices;

    private bool _appStartingUp = true;
    private bool _isShuttingDown = false;
    private AppWindow _appWindow;
    public MainWindow(
        INavigationService navigationService,
        INavigationViewService navigationViewService,
        IAuthenticationService authenticationService,
        IPermissionService permissionService,
        ITenantProfileService tenantProfileService,
        IDialogService dialogService,
        IToastService toastService,
        BackgroundProcessService backgroundServices,
        ILicenseManagerService licenseManagerService,
        DatabaseRestoreService databaseRestoreService)
    {
        InitializeComponent();

        _navigationService = navigationService;
        _navigationViewService = navigationViewService;
        _authenticationService = authenticationService;
        _permissionService = permissionService;
        _tenantProfileService = tenantProfileService;
        _dialogService = dialogService;
        _toastService = toastService;
        _backgroundServices = backgroundServices;
        _licenseManagerService = licenseManagerService;
        _databaseRestoreService = databaseRestoreService;

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

        // Show loading state immediately for startup sequence
        LoginView.SetLoading(true, "Starting background services. This may take a moment...");

        // Hook into Loaded to start sequence
        RootGrid.Loaded += RootGrid_Loaded;

        _authenticationService.AuthenticationChanged += OnAuthenticationChanged;
        _authenticationService.AuthenticationProcessStateChanged += OnAuthenticationProcessStateChanged;
        LoginView.LoginRequested += OnLoginRequested;
        LoginView.RegisterRequested += OnRegisterRequested;
        UserOnboardingView.ViewModel.OnboardingCanceled += OnOnboardingCanceled;
        UserOnboardingView.ViewModel.RegistrationCompleted += OnRegistrationCompleted;

        RestoreView.ParentWindow = this;
        RestoreView.BackgroundProcessService = _backgroundServices;
        RestoreView.DatabaseRestoreService = databaseRestoreService;
        RestoreView.RestoreCanceled += (s, e) =>
        {
            RestoreView.Visibility = Visibility.Collapsed;
            LoginView.Visibility = Visibility.Visible;
        };
        RestoreView.RestoreCompleted += (s, e) =>
        {
            RestoreView.Visibility = Visibility.Collapsed;
            LoginView.Visibility = Visibility.Visible;
            LoginView.SetLoading(false);
            // APIs are back up; user can now log in normally
        };

        LoginView.RestoreRequested += (s, e) => ShowRestoreView();

        // Ensure app shuts down when window is closed
        Closed += (s, e) =>
        {
            // Unsubscribe to prevent memory leak (Singleton holding ref to MainWindow)
            RootGrid.Loaded -= RootGrid_Loaded;
            _authenticationService.AuthenticationChanged -= OnAuthenticationChanged;
            _authenticationService.AuthenticationProcessStateChanged -= OnAuthenticationProcessStateChanged;
            LoginView.LoginRequested -= OnLoginRequested;
            LoginView.RegisterRequested -= OnRegisterRequested;
            UserOnboardingView.ViewModel.OnboardingCanceled -= OnOnboardingCanceled;
            UserOnboardingView.ViewModel.RegistrationCompleted -= OnRegistrationCompleted;

            //_backgroundServices.StopBackendServicesAsync();

            //if (_isShuttingDown)
            //    _ = PerformGracefulShutdownAsync();

            // Make sure to dispose settings or services if needed
            //Microsoft.UI.Xaml.Application.Current.Exit();
        };

        IntPtr hWind = WinRT.Interop.WindowNative.GetWindowHandle(this);
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWind);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        _appWindow.Closing += AppWindow_Closing;
    }

    private void RootGrid_Loaded(object sender, RoutedEventArgs e)
    {
        if (!CheckLicenseStatus())
        {
            // Suspend startup, wait for valid license
            LicenseValidation.ParentWindow = this;
            LicenseValidation.LicenseValidated += LicenseValidationView_LicenseValidated;
            
            LoginView.Visibility = Visibility.Collapsed;
            LicenseValidation.Visibility = Visibility.Visible;
            return;
        }

        // License valid, proceed with normal startup
        _ = StartBackendAndAuthSequenceAsync();
    }

    private void LicenseValidationView_LicenseValidated(object? sender, EventArgs e)
    {
        LicenseValidation.LicenseValidated -= LicenseValidationView_LicenseValidated;
        LicenseValidation.Visibility = Visibility.Collapsed;
        LoginView.Visibility = Visibility.Visible;
        
        _ = StartBackendAndAuthSequenceAsync();
    }

    private async Task StartBackendAndAuthSequenceAsync()
    {
        try
        {
            await _backgroundServices.WaitForApiToWakeUpAsync();
        }
        catch (Exception)
        {
            DispatcherQueue.TryEnqueue(() => 
            {
                LoginView.SetLoading(false);
                LoginView.ShowError("Could not start background services. Please check the logs and restart the app.");
            });
            return;
        }

        DispatcherQueue.TryEnqueue(() => LoginView.SetLoading(true, "Checking authentication..."));
        
        await _authenticationService.InitializeAsync();

        DispatcherQueue.TryEnqueue(() => 
        {
            if (!_authenticationService.IsAuthenticated)
            {
                LoginView.SetLoading(false);
                UpdateAuthenticationUI();
            }
        });
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
            // Block UI update until permissions and tenant profile are fetched.
            // The user will see exactly what they saw during login (the splash/loading state)
            var pTask = _permissionService.InitializeAsync();
            var tTask = _tenantProfileService.InitializeAsync();
            
            await Task.WhenAll(pTask, tTask);

            if (!pTask.Result || !tTask.Result)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    LoginView.ShowError("Initialization Failed: Could not load user session details. Please try again.");
                });

                await _authenticationService.LogoutAsync();
                return;
            }
        }
        else
        {
            _permissionService.ClearPermissions();
            _tenantProfileService.ClearProfile();
        }

        // UI updates must happen on the UI thread
        DispatcherQueue.TryEnqueue(() =>
        {
            UpdateAuthenticationUI();
        });
    }

    private async void OnRegisterRequested(object? sender, EventArgs e)
    {
        LoginView.Visibility = Visibility.Collapsed;
        UserOnboardingView.Visibility = Visibility.Visible;

        // Fetch plans only once when the user navigates directly to the onboarding view
        if (UserOnboardingView.ViewModel.SubscriptionPlans.Count == 0)
        {
            await UserOnboardingView.ViewModel.GetSubscriptionPlansAsync();
        }
    }

    private void OnOnboardingCanceled(object? sender, EventArgs e)
    {
        UserOnboardingView.Visibility = Visibility.Collapsed;
        LoginView.Visibility = Visibility.Visible;
    }

    private void OnRegistrationCompleted(object? sender, EventArgs e)
    {
        UserOnboardingView.Visibility = Visibility.Collapsed;
        LoginView.Visibility = Visibility.Visible;

        // Refresh plans or state if necessary here
    }

    private void OnAuthenticationProcessStateChanged(object? sender, bool isAuthenticating)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            LoginView.SetLoading(isAuthenticating);
            
            // If authentication finished and we are NOT authenticated, restore the login button
            if (!isAuthenticating && !_authenticationService.IsAuthenticated)
            {
                LoginView.UpdateLoginButtonVisibility(false);
            }
        });
    }

    private async void OnLoginRequested(object? sender, EventArgs e)
    {
         await _authenticationService.LoginAsync();
    }

    private bool CheckLicenseStatus()
    {
        string localAppData = ApplicationData.Current.LocalFolder.Path;
        string licensePath = Path.Combine(localAppData, "license.key");

        if(File.Exists(licensePath))
        {
            string keyContents = File.ReadAllText(licensePath);
            if (_licenseManagerService.IsKeyValid(keyContents))
            {
                return true;
            }
        }

        return false;
    }

    private void CheckLoginStatus()
    {
        // Initial check
        UpdateAuthenticationUI();
    }

    private void UpdateAuthenticationUI()
    {
        LoginView.UpdateLoginButtonVisibility(_authenticationService.IsAuthenticated);

        if (_authenticationService.IsAuthenticated)
        {
            // Authenticated State
            LoginView.Visibility = Visibility.Collapsed;
            UserOnboardingView.Visibility = Visibility.Collapsed;
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
            if (UserOnboardingView.Visibility != Visibility.Visible)
            {
                LoginView.Visibility = Visibility.Visible;
                AnimateOpacity(LoginView, 0, 1);
            }
            NavView.Visibility = Visibility.Collapsed;
            UserOnboardingView.Visibility = Visibility.Collapsed;
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

    private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        if (_appStartingUp)
        {
            var appWindow = this.AppWindow;

            if (appWindow.Presenter is OverlappedPresenter overlappedPresenter)
            {
                overlappedPresenter.Maximize();
            }
            _appStartingUp = false;
        }
    }

    private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        // If we are already shutting down, let the window close naturally
        if (_isShuttingDown) return;

        // 1. Cancel the immediate close action!
        args.Cancel = true;

        // 2. Start our custom async shutdown process
        _ = PerformGracefulShutdownAsync();
    }

    private async Task PerformGracefulShutdownAsync()
    {
        bool isBackupEnabled = CheckIfBackupsAreEnabled();

        var dialog = new ContentDialog
        {
            Title = isBackupEnabled ? "Securing your data..." : "Shutting down...",
            Content = new StackPanel
            {
                Spacing = 16,
                Children =
                    {
                        new ProgressRing { IsActive = true, HorizontalAlignment = HorizontalAlignment.Center },
                        new TextBlock
                        {
                            Text = isBackupEnabled
                                ? "Please wait while we create a database backup. The application will close automatically."
                                : "Closing background services safely...",
                            TextWrapping = TextWrapping.Wrap,
                            Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"],
                            TextAlignment = TextAlignment.Center
                        }
                    },
                HorizontalAlignment = HorizontalAlignment.Center
            },
            XamlRoot = this.Content.XamlRoot,
            IsPrimaryButtonEnabled = false, 
            IsSecondaryButtonEnabled = false
        };

        _ = dialog.ShowAsync();

        await _backgroundServices.StopBackendServicesAsync();

        _isShuttingDown = true;

        dialog.Hide();

        Application.Current.Exit();
    }

    private bool CheckIfBackupsAreEnabled()
    {
        try
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string configPath = System.IO.Path.Combine(appData, "OnyxOms", "system_config.json");

            if (!System.IO.File.Exists(configPath)) return false;

            var json = System.IO.File.ReadAllText(configPath);
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("BackupSettings", out var backupSettings))
            {
                if (backupSettings.TryGetProperty("IsEnabled", out var isEnabled) && isEnabled.GetBoolean())
                {
                    if (backupSettings.TryGetProperty("BackupPath", out var path) && !string.IsNullOrWhiteSpace(path.GetString()))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public void ShowRestoreView()
    {
        LoginView.Visibility = Visibility.Collapsed;
        RestoreView.Visibility = Visibility.Visible;
    }
}
