using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Onyx.Oms.Client.Desktop.Features.Settings
{
    public partial class DatabaseBackupViewModel : ObservableObject
    {
        private readonly ILogger<DatabaseBackupViewModel> _logger;

        private static string ConfigPath
        {
            get
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string folder = Path.Combine(appData, "OnyxOms");
                Directory.CreateDirectory(folder);           // no-op if already exists
                return Path.Combine(folder, "system_config.json");
            }
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        private string _backupPath = string.Empty;
        public string BackupPath
        {
            get => _backupPath;
            set => SetProperty(ref _backupPath, value);
        }

        private double _intervalHours = 4;
        public double IntervalHours
        {
            get => _intervalHours;
            set => SetProperty(ref _intervalHours, value);
        }

        // snapshot for cancel
        private bool _originalIsEnabled;
        private string _originalBackupPath = string.Empty;
        private double _originalIntervalHours;
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
        public Visibility EditButtonVisible => !IsEditing ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SaveCancelVisible => IsEditing ? Visibility.Visible : Visibility.Collapsed;

        public DatabaseBackupViewModel()
        {
            _logger = App.Current.Services.GetRequiredService<ILogger<DatabaseBackupViewModel>>();
        }

        public void Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    IsEnabled = false;
                    BackupPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "OnyxBackups");
                    IntervalHours = 4;
                    return;
                }

                string raw = File.ReadAllText(ConfigPath);
                using var doc = JsonDocument.Parse(raw);

                if(doc.RootElement.TryGetProperty("BackupSettings", out var bs))
                {
                    IsEnabled = bs.TryGetProperty("IsEnabled", out var en)
                    && en.GetBoolean();
                    BackupPath = bs.TryGetProperty("BackupPath", out var bp)
                        ? bp.GetString() ?? string.Empty
                        : string.Empty;
                    IntervalHours = bs.TryGetProperty("IntervalHours", out var iv)
                        ? iv.GetInt32()
                        : 4;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load database settings");
            }
        }

        public void BeginEdit()
        {
            _originalIsEnabled = IsEnabled;
            _originalBackupPath = BackupPath;
            _originalIntervalHours = IntervalHours;
            IsEditing = true;
        }
        public void CancelEdit()
        {
            IsEnabled = _originalIsEnabled;
            BackupPath = _originalBackupPath;
            IntervalHours = _originalIntervalHours;
            IsEditing = false;
        }
        public void Save()
        {
            SaveSystemConfiguration(IsEnabled, BackupPath, IntervalHours);
            IsEditing = false;
        }

        private static void SaveSystemConfiguration(bool isEnabled, string backupPath, double intervalHours)
        {
            var config = new
            {
                BackupSettings = new
                {
                    IsEnabled = isEnabled,
                    BackupPath = backupPath,
                    IntervalHours = (int)intervalHours
                }
            };
            string json = JsonSerializer.Serialize(config,
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigPath, json);
        }
    }
}
