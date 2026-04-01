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
        public event EventHandler? OnboardingCanceled;
        public event EventHandler? RegistrationCompleted;

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

        private int _currentStep = 1;
        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                if (SetProperty(ref _currentStep, value))
                {
                    OnPropertyChanged(nameof(IsStep1Visible));
                    OnPropertyChanged(nameof(IsStep2Visible));
                    OnPropertyChanged(nameof(IsStep3Visible));
                }
            }
        }

        public bool IsStep1Visible => CurrentStep == 1;
        public bool IsStep2Visible => CurrentStep == 2;
        public bool IsStep3Visible => CurrentStep == 3;

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
        private SubscriptionPlanDto? _selectedSubscriptionPlan;
        public SubscriptionPlanDto? SelectedSubscriptionPlan
        {
            get => _selectedSubscriptionPlan;
            set => SetProperty(ref _selectedSubscriptionPlan, value);
        }

        public ObservableCollection<SubscriptionPlanDto> SubscriptionPlans = new();

        public IAsyncRelayCommand LoadSubscriptionPlansCommand { get; }
        public IRelayCommand NextCommand { get; }
        public IRelayCommand BackCommand { get; }
        public IRelayCommand CancelCommand { get; }
        public IAsyncRelayCommand RegisterCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }

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
            NextCommand = new RelayCommand(Next);
            BackCommand = new RelayCommand(Back);
            CancelCommand = new RelayCommand(() => OnboardingCanceled?.Invoke(this, EventArgs.Empty));
            RegisterCommand = new AsyncRelayCommand(RegisterAsync);
            RefreshCommand = new AsyncRelayCommand(GetSubscriptionPlansAsync);

            // Fetch plans silently in background so they are ready by step 3
            //LoadSubscriptionPlansCommand.ExecuteAsync(null);
        }

        public async Task GetSubscriptionPlansAsync()
        {
            try
            {
                IsLoading = true;
                var result = await _subscriptionPlansApi.GetSubscriptionsPlanAsync(new GetSubsriptionPlansRequest());
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

        private void Next()
        {
            if (CurrentStep == 1)
            {
                if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    _toastService.ShowError("Validation Error", "Please fill in all user details.");
                    return;
                }
                if (Password != ConfirmPassword)
                {
                    _toastService.ShowError("Validation Error", "Passwords do not match.");
                    return;
                }
                CurrentStep++;
            }
            else if (CurrentStep == 2)
            {
                if (string.IsNullOrWhiteSpace(CompanyName) || string.IsNullOrWhiteSpace(ContactEmail))
                {
                    _toastService.ShowError("Validation Error", "Please fill in all company details.");
                    return;
                }
                CurrentStep++;
            }
        }

        private void Back()
        {
            if (CurrentStep > 1)
            {
                CurrentStep--;
            }
        }

        private async Task RegisterAsync()
        {
            if (SelectedSubscriptionPlan == null)
            {
                _toastService.ShowError("Validation Error", "Please select a subscription plan.");
                return;
            }

            try
            {
                IsLoading = true;
                var request = new RegisterUserRequest
                {
                    UserDetails = new RegisterUserDetailsDto
                    {
                        FirstName = FirstName,
                        LastName = LastName,
                        Email = Email,
                        Password = Password,
                        ConfirmPassword = ConfirmPassword
                    },
                    CompanyDetails = new RegisterComapnyDetailsDto
                    {
                        CompanyName = CompanyName,
                        ContactEmail = ContactEmail
                    },
                    SubscriptionDetails = new RegisterSubscriptionPlanDetailsDto
                    {
                        SubscriptionId = SelectedSubscriptionPlan.Id
                    }
                };

                var newUserId = await _usersApi.RegisterUserAsync(request);
                _toastService.ShowSuccess("Registration Successful", "You can now log in with your new account.");
                RegistrationCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                //_toastService.ShowError("Registration Failed", "An error occurred during registration. Please try again.");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
