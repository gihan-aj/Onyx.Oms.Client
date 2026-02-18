using System;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class TokenStorageService : ITokenStorageService
{
    private const string ResourceName = "Onyx.Oms.Client";
    private const string AccessTokenKey = "AccessToken";
    private const string RefreshTokenKey = "RefreshToken";
    private const string IdTokenKey = "IdToken";

    public Task SaveTokensAsync(string accessToken, string refreshToken, string idToken)
    {
        var vault = new PasswordVault();
        
        vault.Add(new PasswordCredential(ResourceName, AccessTokenKey, accessToken));
        
        if (!string.IsNullOrEmpty(refreshToken))
        {
            vault.Add(new PasswordCredential(ResourceName, RefreshTokenKey, refreshToken));
        }

        if (!string.IsNullOrEmpty(idToken))
        {
            vault.Add(new PasswordCredential(ResourceName, IdTokenKey, idToken));
        }

        return Task.CompletedTask;
    }

    public Task<(string? AccessToken, string? RefreshToken, string? IdToken)> GetTokensAsync()
    {
        var vault = new PasswordVault();
        string? accessToken = null;
        string? refreshToken = null;
        string? idToken = null;

        try
        {
            var accessCred = vault.Retrieve(ResourceName, AccessTokenKey);
            accessCred.RetrievePassword();
            accessToken = accessCred.Password;
        }
        catch (Exception) { /* Not found */ }

        try
        {
            var refreshCred = vault.Retrieve(ResourceName, RefreshTokenKey);
            refreshCred.RetrievePassword();
            refreshToken = refreshCred.Password;
        }
        catch (Exception) { /* Not found */ }

        try
        {
            var idCred = vault.Retrieve(ResourceName, IdTokenKey);
            idCred.RetrievePassword();
            idToken = idCred.Password;
        }
        catch (Exception) { /* Not found */ }

        return Task.FromResult((accessToken, refreshToken, idToken));
    }

    public Task ClearTokensAsync()
    {
        var vault = new PasswordVault();

        try
        {
            var accessCred = vault.Retrieve(ResourceName, AccessTokenKey);
            vault.Remove(accessCred);
        }
        catch (Exception) { /* Not found */ }

        try
        {
            var refreshCred = vault.Retrieve(ResourceName, RefreshTokenKey);
            vault.Remove(refreshCred);
        }
        catch (Exception) { /* Not found */ }

        try
        {
            var idCred = vault.Retrieve(ResourceName, IdTokenKey);
            vault.Remove(idCred);
        }
        catch (Exception) { /* Not found */ }

        return Task.CompletedTask;
    }
}
