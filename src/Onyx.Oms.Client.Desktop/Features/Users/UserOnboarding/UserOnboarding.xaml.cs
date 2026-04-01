using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Onyx.Oms.Client.Desktop.Shared.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Onyx.Oms.Client.Desktop.Features.Users.UserOnboarding
{
    public sealed partial class UserOnboarding : UserControl
    {
        public UserOnboardingViewModel ViewModel { get; }
        public UserOnboarding()
        {
            ViewModel = App.Current.Services.GetRequiredService<UserOnboardingViewModel>();
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.IsLoading = true;

            try
            {
                // Wait for the API to actually be listening on port 5001
                var backgroundService = App.Current.Services.GetService<BackgroundProcessService>();
                if(backgroundService == null)
                {
                    Log.Error("BackgroundProcessService is not registered in the service container.");
                    return;
                }
                await backgroundService.WaitForApiToWakeUpAsync();

                // NOW it is safe to make Refit calls!
                await ViewModel.GetSubscriptionPlansAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load onboarding data.");
            }
            finally
            {
                ViewModel.IsLoading = false;
            }
        }
    }
}
