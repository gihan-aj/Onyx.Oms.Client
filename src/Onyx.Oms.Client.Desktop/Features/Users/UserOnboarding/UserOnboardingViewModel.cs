using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Users.UserOnboarding
{
    public partial class UserOnboardingViewModel : ObservableObject
    {
        private readonly ISubscriptionPlansApi _subscriptionPlansApi;
        private readonly IUsersApi _usersApi;
        private readonly IToastService _toastService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<UserOnboardingViewModel> _logger;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // User details
        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        // Company details
        private string _companyName = string.Empty;
        public string CompanyName
        {
            get => _companyName;
            set => SetProperty(ref _companyName, value);
        }

        private string _contactEmail = string.Empty;
        public string ContactEmail
        {
            get => _contactEmail;
            set => SetProperty(ref _contactEmail, value);
        }

        // Subscription plan
        private string _selectedSubscriptionPlanId = string.Empty;
        public string SelectedSubscriptionPlanId
        {
            get => _selectedSubscriptionPlanId;
            set => SetProperty(ref _selectedSubscriptionPlanId, value);
        }

        public ObservableCollection<SubscriptionPlanDto> SubscriptionPlans = new();

        public IAsyncRelayCommand LoadSubscriptionPlansCommand { get; }

        public UserOnboardingViewModel(
            ISubscriptionPlansApi subscriptionPlansApi, 
            IUsersApi usersApi, 
            IToastService toastService, 
            IDialogService dialogService, 
            ILogger<UserOnboardingViewModel> logger)
        {
            _subscriptionPlansApi = subscriptionPlansApi;
            _usersApi = usersApi;
            _toastService = toastService;
            _dialogService = dialogService;
            _logger = logger;

            LoadSubscriptionPlansCommand = new AsyncRelayCommand(GetSubscriptionPlansAsync);

            LoadSubscriptionPlansCommand.ExecuteAsync(null);
        }

        private async Task GetSubscriptionPlansAsync()
        {
            if(IsLoading) return;

            try
            {
                IsLoading = true;
                var result = await _subscriptionPlansApi.GetSubscriptionsPlanAsync();
                foreach (var subscriptionPlanDto in result)
                {
                    SubscriptionPlans.Add(subscriptionPlanDto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading subscription plans");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
