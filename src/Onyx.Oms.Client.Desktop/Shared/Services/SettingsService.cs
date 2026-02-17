using Windows.Storage;
using Windows.Security.Credentials;
using System.Linq;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class SettingsService : ISettingsService
{
    private readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;
    private const string ResourceName = "OnyxOmsClient"; // Resource name for PasswordVault

    public T? GetValue<T>(string key, T? defaultValue = default)
    {
        if (_localSettings.Values.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        return defaultValue;
    }

    public void SetValue<T>(string key, T value)
    {
        _localSettings.Values[key] = value;
    }

    public void SetSecret(string key, string secret)
    {
        var vault = new PasswordVault();
        var credential = new PasswordCredential(ResourceName, key, secret);
        vault.Add(credential);
    }

    public string? GetSecret(string key)
    {
        var vault = new PasswordVault();
        try
        {
            // PasswordVault throws if not found
            var credential = vault.Retrieve(ResourceName, key);
            credential.RetrievePassword();
            return credential.Password;
        }
        catch
        {
            return null;
        }
    }

    public void RemoveSecret(string key)
    {
        var vault = new PasswordVault();
        try
        {
            var credential = vault.Retrieve(ResourceName, key);
            vault.Remove(credential);
        }
        catch
        {
            // Ignore if not found
        }
    }
}
