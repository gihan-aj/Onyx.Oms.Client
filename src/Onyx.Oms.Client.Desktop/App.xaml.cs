using CommunityToolkit.WinUI.Animations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using Onyx.Oms.Client.Desktop.Features.Catalog;
using Onyx.Oms.Client.Desktop.Features.Couriers;
using Onyx.Oms.Client.Desktop.Features.Dashboard;
using Onyx.Oms.Client.Desktop.Features.Customers;
using Onyx.Oms.Client.Desktop.Features.ProductCategories;
using Onyx.Oms.Client.Desktop.Features.Products;
using Onyx.Oms.Client.Desktop.Features.Roles;
using Onyx.Oms.Client.Desktop.Features.Settings.Services;
using Onyx.Oms.Client.Desktop.Features.Users;
using Onyx.Oms.Client.Desktop.Features.Users.UserOnboarding;
using Onyx.Oms.Client.Desktop.Shared.Models.Configuration;
using Onyx.Oms.Client.Desktop.Shared.Services;
using Onyx.Oms.Client.Desktop.Shared.Services.Http;
using Refit;
using Serilog;
using System;
using Onyx.Oms.Client.Desktop.Features.FulfillmentTasks;
using Onyx.Oms.Client.Desktop.Features.Orders;

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
        public static Window MainWindow { get; set; } = new Window();

        // Expose ServiceProvider
        public IServiceProvider Services { get; }

        public App()
        {
            InitializeComponent();
            
            ConfigureLogging();

            Services = ConfigureServices();
        }

        private void ConfigureLogging()
        {
            var logFolder = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Logs");
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Refit", Serilog.Events.LogEventLevel.Warning)
                .WriteTo.Debug()
                .WriteTo.File(System.IO.Path.Combine(logFolder, "log-.txt"), 
                    rollingInterval: RollingInterval.Day, 
                    retainedFileCountLimit: 7)
                .CreateLogger();

            Log.Information("Application Starting Up");
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Build Configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

#if DEBUG
            builder.AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);
#endif

            var configuration = builder.Build();

            services.AddSingleton<IConfiguration>(configuration);

            // Configure Options
            services.Configure<AuthenticationOptions>(configuration.GetSection(AuthenticationOptions.SectionName));
            services.Configure<OnyxOmsApiOptions>(configuration.GetSection(OnyxOmsApiOptions.SectionName));

            // Logging
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            // Core Infrastructure
            services.AddSingleton<ISettingsService, SettingsService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<ITokenStorageService, TokenStorageService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();

            // Core Services
            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<INavigationViewService, NavigationViewService>();
            services.AddSingleton<IPermissionService, PermissionService>();
            services.AddSingleton<ITenantProfileService, TenantProfileService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IToastService, ToastService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddScoped<ILicenseManagerService, LicenseManagerService>();

            // HTTP Infrastructure
            services.AddTransient<HttpLoggingHandler>();
            services.AddTransient<AuthHeaderHandler>();
            services.AddTransient<ProblemDetailsHandler>();

            // Wireup Backend Services
            services.AddSingleton<BackgroundProcessService>();

            // API Clients
            services.AddRefitClient<ICourierApi>()
                    .ConfigureHttpClient((sp, c) => 
                    {
                        var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
                        c.BaseAddress = new Uri(options.BaseUrl);
                    })
                    .AddHttpMessageHandler<HttpLoggingHandler>()
                    .AddHttpMessageHandler<AuthHeaderHandler>()
                    .AddHttpMessageHandler<ProblemDetailsHandler>();

            services.AddRefitClient<IUserApi>()
                    .ConfigureHttpClient((sp, c) => 
                    {
                        var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
                        c.BaseAddress = new Uri(options.BaseUrl);
                    })
                    .AddHttpMessageHandler<HttpLoggingHandler>()
                    .AddHttpMessageHandler<AuthHeaderHandler>()
                    .AddHttpMessageHandler<ProblemDetailsHandler>();
            
            services.AddRefitClient<IRoleApi>()
                    .ConfigureHttpClient((sp, c) => 
                    {
                        var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
                        c.BaseAddress = new Uri(options.BaseUrl);
                    })
                    .AddHttpMessageHandler<HttpLoggingHandler>()
                    .AddHttpMessageHandler<AuthHeaderHandler>()
                    .AddHttpMessageHandler<ProblemDetailsHandler>();

            services.AddRefitClient<ICustomerApi>()
                    .ConfigureHttpClient((sp, c) => 
                    {
                        var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
                        c.BaseAddress = new Uri(options.BaseUrl);
                    })
                    .AddHttpMessageHandler<HttpLoggingHandler>()
                    .AddHttpMessageHandler<AuthHeaderHandler>()
                    .AddHttpMessageHandler<ProblemDetailsHandler>();
                    
            services.AddRefitClient<IProductCategoryApi>()
                    .ConfigureHttpClient((sp, c) => 
                    {
                        var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
                        c.BaseAddress = new Uri(options.BaseUrl);
                    })
                    .AddHttpMessageHandler<HttpLoggingHandler>()
                    .AddHttpMessageHandler<AuthHeaderHandler>()
                    .AddHttpMessageHandler<ProblemDetailsHandler>();

            services.AddRefitClient<IProductsApi>()
                    .ConfigureHttpClient((sp, c) =>
                    {
                        var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
                        c.BaseAddress = new Uri(options.BaseUrl);
                    })
                    .AddHttpMessageHandler<HttpLoggingHandler>()
                    .AddHttpMessageHandler<AuthHeaderHandler>()
                    .AddHttpMessageHandler<ProblemDetailsHandler>();

            services.AddRefitClient<IFulfillmentTasksApi>()
                    .ConfigureHttpClient((sp, c) =>
                    {
                        var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
                        c.BaseAddress = new Uri(options.BaseUrl);
                    })
                    .AddHttpMessageHandler<HttpLoggingHandler>()
                    .AddHttpMessageHandler<AuthHeaderHandler>()
                    .AddHttpMessageHandler<ProblemDetailsHandler>();

            services.AddRefitClient<IOrdersApi>()
                    .ConfigureHttpClient((sp, c) =>
                    {
                        var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
                        c.BaseAddress = new Uri(options.BaseUrl);
                    })
                    .AddHttpMessageHandler<HttpLoggingHandler>()
                    .AddHttpMessageHandler<AuthHeaderHandler>()
                    .AddHttpMessageHandler<ProblemDetailsHandler>();

            services.AddRefitClient<IUsersApi>()
                    .ConfigureHttpClient((sp, c) =>
                    {
                        var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
                        c.BaseAddress = new Uri(options.BaseUrl);
                    })
                    .AddHttpMessageHandler<HttpLoggingHandler>()
                    .AddHttpMessageHandler<AuthHeaderHandler>()
                    .AddHttpMessageHandler<ProblemDetailsHandler>();

            services.AddRefitClient<ISubscriptionPlansApi>()
                    .ConfigureHttpClient((sp, c) =>
                    {
                        var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
                        c.BaseAddress = new Uri(options.BaseUrl);
                    })
                    .AddHttpMessageHandler<HttpLoggingHandler>()
                    .AddHttpMessageHandler<AuthHeaderHandler>()
                    .AddHttpMessageHandler<ProblemDetailsHandler>();

            //services.AddRefitClient<IUpdateProductApi>()
            //        .ConfigureHttpClient((sp, c) => 
            //        {
            //            var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
            //            c.BaseAddress = new Uri(options.BaseUrl);
            //        })
            //        .AddHttpMessageHandler<HttpLoggingHandler>()
            //        .AddHttpMessageHandler<AuthHeaderHandler>()
            //        .AddHttpMessageHandler<ProblemDetailsHandler>();

            services.AddRefitClient<IProductCategoryLookupApi>()
                    .ConfigureHttpClient((sp, c) =>
                    {
                        var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
                        c.BaseAddress = new Uri(options.BaseUrl);
                    })
                    .AddHttpMessageHandler<HttpLoggingHandler>()
                    .AddHttpMessageHandler<AuthHeaderHandler>()
                    .AddHttpMessageHandler<ProblemDetailsHandler>();

            services.AddRefitClient<ICatalogApi>()
                    .ConfigureHttpClient((sp, c) => 
                    {
                        var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
                        c.BaseAddress = new Uri(options.BaseUrl);
                    })
                    .AddHttpMessageHandler<HttpLoggingHandler>()
                    .AddHttpMessageHandler<AuthHeaderHandler>()
                    .AddHttpMessageHandler<ProblemDetailsHandler>();

            services.AddRefitClient<IDashboardApi>()
                    .ConfigureHttpClient((sp, c) => 
                    {
                        var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
                        c.BaseAddress = new Uri(options.BaseUrl);
                    })
                    .AddHttpMessageHandler<HttpLoggingHandler>()
                    .AddHttpMessageHandler<AuthHeaderHandler>()
                    .AddHttpMessageHandler<ProblemDetailsHandler>();
            
            services.AddRefitClient<ITenantProfileApi>()
                    .ConfigureHttpClient((sp, c) => 
                    {
                        var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
                        c.BaseAddress = new Uri(options.BaseUrl);
                    })
                    .AddHttpMessageHandler<HttpLoggingHandler>()
                    .AddHttpMessageHandler<AuthHeaderHandler>()
                    .AddHttpMessageHandler<ProblemDetailsHandler>();

            services.AddRefitClient<IAppSequenceApi>()
                    .ConfigureHttpClient((sp, c) => 
                    {
                        var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
                        c.BaseAddress = new Uri(options.BaseUrl);
                    })
                    .AddHttpMessageHandler<HttpLoggingHandler>()
                    .AddHttpMessageHandler<AuthHeaderHandler>()
                    .AddHttpMessageHandler<ProblemDetailsHandler>();
            
            // Activation Handlers
            // Register specific activation handlers here
            // services.AddTransient<IActivationHandler, SomeSpecificHandler>();

            // Default Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Views and ViewModels
            services.AddTransient<Shared.Shell.MainWindow>();
            // services.AddTransient<ShellViewModel>(); // If we have one
            
            services.AddTransient<Features.Customers.CustomersViewModel>();
            services.AddTransient<Features.Customers.CustomersPage>();
            services.AddTransient<Features.Customers.CustomerFormViewModel>();
            services.AddTransient<Features.Customers.CustomerFormPage>();
            services.AddTransient<Features.Customers.CustomerDetailsDialog>();
            services.AddTransient<Features.Couriers.CouriersViewModel>();
            services.AddTransient<Features.Roles.RolesViewModel>();
            services.AddTransient<Features.Roles.RoleFormViewModel>();
            services.AddTransient<Features.Roles.RoleDetailsDialog>();
            
            services.AddTransient<Features.Dashboard.DashboardPage>();
            services.AddTransient<Features.Dashboard.DashboardViewModel>();
            services.AddTransient<Features.Orders.List.OrdersPage>();
            services.AddTransient<Features.Orders.List.OrdersViewModel>();
            services.AddTransient<Features.Orders.Create.CreateOrderPage>();
            services.AddTransient<Features.Orders.Create.CreateOrderViewModel>();
            services.AddTransient<Features.Orders.Edit.EditOrderPage>();
            services.AddTransient<Features.Orders.Edit.EditOrderViewModel>();
            services.AddTransient<Features.Orders.ProductPicker.ProductPickerViewModel>();
            services.AddTransient<Features.Customers.CustomersPage>();
            services.AddTransient<Features.FulfillmentTasks.List.FulfillmentTasksPage>();
            services.AddTransient<Features.FulfillmentTasks.List.FulfillmentTasksViewModel>();
            services.AddTransient<Features.FulfillmentTasks.Create.CreateFulfillmentTaskPage>();
            services.AddTransient<Features.FulfillmentTasks.Create.CreateFulfillmentTaskViewModel>();
            services.AddTransient<Features.FulfillmentTasks.Edit.EditFulfillmentTaskPage>();
            services.AddTransient<Features.FulfillmentTasks.Edit.EditFulfillmentTaskViewModel>();
            services.AddTransient<Features.FulfillmentTasks.ProductPicker.ProductPickerViewModel>();
            services.AddTransient<Features.Catalog.CatalogPage>();
            services.AddTransient<Features.Catalog.CatalogViewModel>();
            services.AddTransient<Features.ProductCategories.ProductCategoriesPage>();
            services.AddTransient<Features.ProductCategories.ProductCategoriesViewModel>();
            services.AddTransient<Features.ProductCategories.ProductCategoryFormPage>();
            services.AddTransient<Features.ProductCategories.ProductCategoryFormViewModel>();
            services.AddTransient<Features.Products.List.ProductsPage>();
            services.AddTransient<Features.Products.List.ProductsViewModel>();
            services.AddTransient<Features.Products.Create.CreateProductPage>();
            services.AddTransient<Features.Products.Create.CreateProductViewModel>();
            services.AddTransient<Features.Products.Details.ProductDetailsPage>();
            services.AddTransient<Features.Products.Details.ProductDetailsViewModel>();
            services.AddTransient<Features.Products.Edit.EditProductPage>();
            services.AddTransient<Features.Products.Edit.EditProductViewModel>();
            services.AddTransient<Features.ProductVariants.ProductVariantsPage>();
            services.AddTransient<Features.Couriers.CouriersPage>();
            services.AddTransient<Features.Couriers.CourierFormPage>();
            services.AddTransient<Features.Couriers.CourierFormViewModel>();
            services.AddTransient<Features.Users.UsersPage>();
            services.AddTransient<Features.Roles.RolesPage>();
            services.AddTransient<Features.Roles.RoleFormPage>();
            services.AddTransient<Features.Settings.SettingsPage>();
            services.AddTransient<Features.Settings.SettingsViewModel>();
            services.AddTransient<Features.Users.UserOnboarding.UserOnboardingViewModel>();
            // Add other pages

            // Configuration
            // services.Configure<AppConfig>(...);

            var serviceProvider = services.BuildServiceProvider();

            // Configure PageService
            var pageService = serviceProvider.GetRequiredService<IPageService>();
            pageService.Configure(typeof(Features.Dashboard.DashboardPage).FullName!, typeof(Features.Dashboard.DashboardPage));
            pageService.Configure(typeof(Features.Orders.List.OrdersPage).FullName!, typeof(Features.Orders.List.OrdersPage));
            pageService.Configure(typeof(Features.Orders.Create.CreateOrderViewModel).FullName!, typeof(Features.Orders.Create.CreateOrderPage));
            pageService.Configure(typeof(Features.Orders.Edit.EditOrderViewModel).FullName!, typeof(Features.Orders.Edit.EditOrderPage));
            pageService.Configure(typeof(Features.Customers.CustomersPage).FullName!, typeof(Features.Customers.CustomersPage));
            pageService.Configure(typeof(Features.Customers.CustomerFormPage).FullName!, typeof(Features.Customers.CustomerFormPage));
            pageService.Configure(typeof(Features.FulfillmentTasks.List.FulfillmentTasksPage).FullName!, typeof(Features.FulfillmentTasks.List.FulfillmentTasksPage));
            pageService.Configure(typeof(Features.FulfillmentTasks.Create.CreateFulfillmentTaskPage).FullName!, typeof(Features.FulfillmentTasks.Create.CreateFulfillmentTaskPage));
            pageService.Configure(typeof(Features.FulfillmentTasks.Edit.EditFulfillmentTaskViewModel).FullName!, typeof(Features.FulfillmentTasks.Edit.EditFulfillmentTaskPage));
            pageService.Configure(typeof(Features.Catalog.CatalogPage).FullName!, typeof(Features.Catalog.CatalogPage));
            pageService.Configure(typeof(Features.ProductCategories.ProductCategoriesPage).FullName!, typeof(Features.ProductCategories.ProductCategoriesPage));
            pageService.Configure(typeof(Features.ProductCategories.ProductCategoryFormViewModel).FullName!, typeof(Features.ProductCategories.ProductCategoryFormPage));
            pageService.Configure(typeof(Features.Products.List.ProductsPage).FullName!, typeof(Features.Products.List.ProductsPage));
            pageService.Configure(typeof(Features.Products.Create.CreateProductViewModel).FullName!, typeof(Features.Products.Create.CreateProductPage));
            pageService.Configure(typeof(Features.Products.Details.ProductDetailsViewModel).FullName!, typeof(Features.Products.Details.ProductDetailsPage));
            pageService.Configure(typeof(Features.Products.Edit.EditProductViewModel).FullName!, typeof(Features.Products.Edit.EditProductPage));
            pageService.Configure(typeof(Features.ProductVariants.ProductVariantsPage).FullName!, typeof(Features.ProductVariants.ProductVariantsPage));
            pageService.Configure(typeof(Features.Couriers.CouriersPage).FullName!, typeof(Features.Couriers.CouriersPage));
            pageService.Configure(typeof(Features.Couriers.CourierFormViewModel).FullName!, typeof(Features.Couriers.CourierFormPage));
            pageService.Configure(typeof(Features.Users.UsersPage).FullName!, typeof(Features.Users.UsersPage));
            pageService.Configure(typeof(Features.Roles.RolesPage).FullName!, typeof(Features.Roles.RolesPage));
            pageService.Configure(typeof(Features.Roles.RoleFormPage).FullName!, typeof(Features.Roles.RoleFormPage));
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

            // Start Background APIs
            var backgroundServices = Services.GetRequiredService<BackgroundProcessService>();
            try
            {
                backgroundServices.StartBackendServices();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to start APIs: {ex.Message}");
            }

            // Activate the service
            var activationService = Services.GetRequiredService<IActivationService>();
            await activationService.ActivateAsync(args);

            // Initialize Theme
            var themeSelector = Services.GetRequiredService<IThemeSelectorService>();
            await ((ThemeSelectorService)themeSelector).InitializeAsync();

            // Authentication initialization is now handled by MainWindow.xaml.cs during RootGrid.Loaded
            // to allow showing a loading state on the UI.
        }
    }
}
