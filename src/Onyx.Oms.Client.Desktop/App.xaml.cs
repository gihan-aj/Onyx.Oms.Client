 using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Onyx.Oms.Client.Desktop
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;

        // Expose ServiceProvider
        public IServiceProvider Services { get; }

        public App()
        {
            InitializeComponent();

            Services = ConfigureServices();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Core Services
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<INavigationViewService, NavigationViewService>();
            services.AddSingleton<IPermissionService, PermissionService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IPageService, PageService>();

            // Activation Handlers
            // Register specific activation handlers here
            // services.AddTransient<IActivationHandler, SomeSpecificHandler>();

            // Default Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>(); // We need to create this

            // Views and ViewModels
            services.AddTransient<Shared.Shell.MainWindow>();
            // services.AddTransient<ShellViewModel>(); // If we have one
            
            services.AddTransient<Features.Dashboard.DashboardPage>();
            services.AddTransient<Features.Orders.OrdersPage>();
            services.AddTransient<Features.Customers.CustomersPage>();
            services.AddTransient<Features.Settings.SettingsPage>();
            // Add other pages

            // Configuration
            // services.Configure<AppConfig>(...);

            var serviceProvider = services.BuildServiceProvider();

            // Configure PageService
            var pageService = serviceProvider.GetRequiredService<IPageService>();
            pageService.Configure(typeof(Features.Dashboard.DashboardPage).FullName!, typeof(Features.Dashboard.DashboardPage));
            pageService.Configure(typeof(Features.Orders.OrdersPage).FullName!, typeof(Features.Orders.OrdersPage));
            pageService.Configure(typeof(Features.Customers.CustomersPage).FullName!, typeof(Features.Customers.CustomersPage));
            pageService.Configure(typeof(Features.Settings.SettingsPage).FullName!, typeof(Features.Settings.SettingsPage));

            return serviceProvider;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);
            // Activate the service
            var activationService = Services.GetRequiredService<IActivationService>();
            await activationService.ActivateAsync(args);
        }
    }
}
