using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Settings
{
    public partial class WhatsAppSettingsViewModel : ObservableObject
    {
        private readonly ISettingsApi _settingsApi;
        private readonly IToastService _toastService;
        private readonly ILogger<WhatsAppSettingsViewModel> _logger;

        private string? _phoneNumberId;
        public string? PhoneNumberId
        {
            get => _phoneNumberId;
            set => SetProperty(ref _phoneNumberId, value);
        }

        private string? _accessToken;
        public string? AccessToken
        {
            get => _accessToken;
            set => SetProperty(ref _accessToken, value);
        }

        private bool _isConfigured;
        public bool IsConfigured
        {
            get => _isConfigured;
            private set
            {
                if (SetProperty(ref _isConfigured, value))
                {
                    OnPropertyChanged(nameof(StatusSeverity));
                    OnPropertyChanged(nameof(StatusTitle));
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
        }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (SetProperty(ref _isEditing, value))
                {
                    OnPropertyChanged(nameof(EditButtonVisible));
                    OnPropertyChanged(nameof(SaveCancelVisible));
                }
            }
        }
        private bool _canEdit = true;
        public bool CanEdit
        {
            get => _canEdit;
            set
            {
                if (SetProperty(ref _canEdit, value))
                {
                    OnPropertyChanged(nameof(EditButtonVisible));
                }
            }
        }

        public Visibility EditButtonVisible => (CanEdit && !IsEditing) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SaveCancelVisible => IsEditing ? Visibility.Visible : Visibility.Collapsed;
        // Dynamics for InfoBar Connection Status Banner
        public InfoBarSeverity StatusSeverity => IsConfigured ? InfoBarSeverity.Success : InfoBarSeverity.Warning;
        public string StatusTitle => IsConfigured ? "WhatsApp Integration Active" : "WhatsApp Integration Not Configured";
        public string StatusMessage => IsConfigured
            ? "Your system is successfully connected to the WhatsApp Business API."
            : "Please configure your WhatsApp Phone Number ID and Access Token to enable automated WhatsApp notifications.";

        private string? _originalPhoneNumberId;

        private string? _testPhoneNumber;
        public string? TestPhoneNumber
        {
            get => _testPhoneNumber;
            set => SetProperty(ref _testPhoneNumber, value);
        }
        private bool _isTestingConnection;
        public bool IsTestingConnection
        {
            get => _isTestingConnection;
            set => SetProperty(ref _isTestingConnection, value);
        }

        public WhatsAppSettingsViewModel(ISettingsApi settingsApi, IToastService toastService)
        {
            _settingsApi = settingsApi;
            _toastService = toastService;
            _logger = App.Current.Services.GetRequiredService<ILogger<WhatsAppSettingsViewModel>>();
        }

        public async Task LoadSettingsAsync()
        {
            try
            {
                var settings = await _settingsApi.GetWhatsAppSettings();
                PhoneNumberId = settings.PhoneNumberId;
                IsConfigured = settings.IsConfigured;
                AccessToken = string.Empty; // Keep sensitive token empty locally
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load WhatsApp settings.");
            }
        }

        [RelayCommand]
        private void Edit()
        {
            _originalPhoneNumberId = PhoneNumberId;
            AccessToken = string.Empty; // Allow user to override or enter a new token
            IsEditing = true;
        }

        [RelayCommand]
        private void Cancel()
        {
            PhoneNumberId = _originalPhoneNumberId;
            AccessToken = string.Empty;
            IsEditing = false;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(PhoneNumberId))
            {
                _toastService.ShowError("Validation Error", "Phone Number ID is required.");
                return;
            }
            try
            {
                IsEditing = false;
                var command = new UpdateWhatsAppSettingsCommand(PhoneNumberId, AccessToken);
                await _settingsApi.UpdateWhatsAppSettings(command);
                _toastService.ShowSuccess("Success", "WhatsApp settings updated successfully.");

                await LoadSettingsAsync(); // Refresh state from API
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save WhatsApp settings.");
                //_toastService.ShowError("Error", "Failed to save WhatsApp settings.");
                Cancel(); // Rollback to original state
            }
        }

        [RelayCommand]
        private async Task TestConnectionAsync()
        {
            if (string.IsNullOrWhiteSpace(TestPhoneNumber))
            {
                _toastService.ShowError("Validation Error", "Please enter a destination phone number to test the connection.");
                return;
            }
            try
            {
                IsTestingConnection = true;

                var command = new TestWhatsAppConnectionCommand(TestPhoneNumber);
                await _settingsApi.TestWhatsAppConnection(command);

                _toastService.ShowSuccess("Success", "Test WhatsApp message sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test WhatsApp message.");
                //_toastService.ShowError("Connection Failed", "Failed to send test WhatsApp message. Please check the logs.");
            }
            finally
            {
                IsTestingConnection = false;
            }
        }
    }
}
