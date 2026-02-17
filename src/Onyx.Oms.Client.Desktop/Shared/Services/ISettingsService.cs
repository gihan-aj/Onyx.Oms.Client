namespace Onyx.Oms.Client.Desktop.Shared.Services;

public interface ISettingsService
{
    T? GetValue<T>(string key, T? defaultValue = default);
    void SetValue<T>(string key, T value);
    
    // Secure storage for tokens
    void SetSecret(string key, string secret);
    string? GetSecret(string key);
    void RemoveSecret(string key);
}
