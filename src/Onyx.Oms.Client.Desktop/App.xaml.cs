using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml;
using Onyx.Oms.Client.Desktop.Features.Catalog;
using Onyx.Oms.Client.Desktop.Features.Couriers;
using Onyx.Oms.Client.Desktop.Features.Customers;
//using Onyx.Oms.Client.Desktop.Features.Products;
using Onyx.Oms.Client.Desktop.Features.ProductCategories;
using Onyx.Oms.Client.Desktop.Features.Roles;
using Onyx.Oms.Client.Desktop.Features.Settings.Services;
using Onyx.Oms.Client.Desktop.Shared.Models.Configuration;
using Onyx.Oms.Client.Desktop.Shared.Services;
using Onyx.Oms.Client.Desktop.Shared.Services.Http;
using Refit;
using Serilog;
using System;

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
            builder.AddJsonFile("appsettings.development.json", optional: true, reloadOnChange: true);
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

            // HTTP Infrastructure
            services.AddTransient<HttpLoggingHandler>();
            services.AddTransient<AuthHeaderHandler>();
            services.AddTransient<ProblemDetailsHandler>();

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

            //services.AddRefitClient<IGetProductsApi>()
            //        .ConfigureHttpClient((sp, c) => 
            //        {
            //            var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
            //            c.BaseAddress = new Uri(options.BaseUrl);
            //        })
            //        .AddHttpMessageHandler<HttpLoggingHandler>()
            //        .AddHttpMessageHandler<AuthHeaderHandler>()
            //        .AddHttpMessageHandler<ProblemDetailsHandler>();
            
            //services.AddRefitClient<IGetProductDetailsApi>()
            //        .ConfigureHttpClient((sp, c) => 
            //        {
            //            var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
            //            c.BaseAddress = new Uri(options.BaseUrl);
            //        })
            //        .AddHttpMessageHandler<HttpLoggingHandler>()
            //        .AddHttpMessageHandler<AuthHeaderHandler>()
            //        .AddHttpMessageHandler<ProblemDetailsHandler>();

            //services.AddRefitClient<ICreateProductApi>()
            //        .ConfigureHttpClient((sp, c) => 
            //        {
            //            var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
            //            c.BaseAddress = new Uri(options.BaseUrl);
            //        })
            //        .AddHttpMessageHandler<HttpLoggingHandler>()
            //        .AddHttpMessageHandler<AuthHeaderHandler>()
            //        .AddHttpMessageHandler<ProblemDetailsHandler>();

            //services.AddRefitClient<IUpdateProductApi>()
            //        .ConfigureHttpClient((sp, c) => 
            //        {
            //            var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
            //            c.BaseAddress = new Uri(options.BaseUrl);
            //        })
            //        .AddHttpMessageHandler<HttpLoggingHandler>()
            //        .AddHttpMessageHandler<AuthHeaderHandler>()
            //        .AddHttpMessageHandler<ProblemDetailsHandler>();
            
            //services.AddRefitClient<IProductCategoryLookupApi>()
            //        .ConfigureHttpClient((sp, c) => 
            //        {
            //            var options = sp.GetRequiredService<IOptions<OnyxOmsApiOptions>>().Value;
            //            c.BaseAddress = new Uri(options.BaseUrl);
            //        })
            //        .AddHttpMessageHandler<HttpLoggingHandler>()
            //        .AddHttpMessageHandler<AuthHeaderHandler>()
            //        .AddHttpMessageHandler<ProblemDetailsHandler>();

            services.AddRefitClient<ICatalogApi>()
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
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>(); // We need to create this

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
            services.AddTransient<Features.Orders.OrdersPage>();
            services.AddTransient<Features.Customers.CustomersPage>();
            services.AddTransient<Features.Fulfillment.FulfillmentPage>();
            services.AddTransient<Features.Catalog.CatalogPage>();
            services.AddTransient<Features.Catalog.CatalogViewModel>();
            services.AddTransient<Features.ProductCategories.ProductCategoriesPage>();
            services.AddTransient<Features.ProductCategories.ProductCategoriesViewModel>();
            services.AddTransient<Features.ProductCategories.ProductCategoryFormPage>();
            services.AddTransient<Features.ProductCategories.ProductCategoryFormViewModel>();
            //services.AddTransient<Features.Products.ProductsPage>();
            //services.AddTransient<Features.Products.ProductsViewModel>();
            //services.AddTransient<Features.Products.CreateProductPage>();
            //services.AddTransient<Features.Products.CreateProductViewModel>();
            //services.AddTransient<Features.Products.ProductDetailsPage>();
            //services.AddTransient<Features.Products.ProductDetailsViewModel>();
            //services.AddTransient<Features.Products.EditProductPage>();
            //services.AddTransient<Features.Products.EditProductViewModel>();
            services.AddTransient<Features.ProductVariants.ProductVariantsPage>();
            services.AddTransient<Features.Couriers.CouriersPage>();
            services.AddTransient<Features.Couriers.CourierFormPage>();
            services.AddTransient<Features.Couriers.CourierFormViewModel>();
            services.AddTransient<Features.Users.UsersPage>();
            services.AddTransient<Features.Roles.RolesPage>();
            services.AddTransient<Features.Roles.RoleFormPage>();
            services.AddTransient<Features.Settings.SettingsPage>();
            services.AddTransient<Features.Settings.SettingsViewModel>();
            // Add other pages

            // Configuration
            // services.Configure<AppConfig>(...);

            var serviceProvider = services.BuildServiceProvider();

            // Configure PageService
            var pageService = serviceProvider.GetRequiredService<IPageService>();
            pageService.Configure(typeof(Features.Dashboard.DashboardPage).FullName!, typeof(Features.Dashboard.DashboardPage));
            pageService.Configure(typeof(Features.Orders.OrdersPage).FullName!, typeof(Features.Orders.OrdersPage));
            pageService.Configure(typeof(Features.Customers.CustomersPage).FullName!, typeof(Features.Customers.CustomersPage));
            pageService.Configure(typeof(Features.Customers.CustomerFormPage).FullName!, typeof(Features.Customers.CustomerFormPage));
            pageService.Configure(typeof(Features.Fulfillment.FulfillmentPage).FullName!, typeof(Features.Fulfillment.FulfillmentPage));
            pageService.Configure(typeof(Features.Catalog.CatalogPage).FullName!, typeof(Features.Catalog.CatalogPage));
            pageService.Configure(typeof(Features.ProductCategories.ProductCategoriesPage).FullName!, typeof(Features.ProductCategories.ProductCategoriesPage));
            pageService.Configure(typeof(Features.ProductCategories.ProductCategoryFormViewModel).FullName!, typeof(Features.ProductCategories.ProductCategoryFormPage));
            //pageService.Configure(typeof(Features.Products.ProductsPage).FullName!, typeof(Features.Products.ProductsPage));
            //pageService.Configure(typeof(Features.Products.CreateProductViewModel).FullName!, typeof(Features.Products.CreateProductPage));
            //pageService.Configure(typeof(Features.Products.ProductDetailsViewModel).FullName!, typeof(Features.Products.ProductDetailsPage));
            //pageService.Configure(typeof(Features.Products.EditProductViewModel).FullName!, typeof(Features.Products.EditProductPage));
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
            
            // Activate the service
            var activationService = Services.GetRequiredService<IActivationService>();
            await activationService.ActivateAsync(args);

            // Initialize Theme
            var themeSelector = Services.GetRequiredService<IThemeSelectorService>();
            await ((ThemeSelectorService)themeSelector).InitializeAsync();

            // Initialize Authentication
            var authService = Services.GetRequiredService<IAuthenticationService>();
            await authService.InitializeAsync();
        }
    }
}
