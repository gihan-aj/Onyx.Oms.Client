using Duende.IdentityModel.OidcClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Onyx.Oms.Client.Desktop.Shared.Models.Configuration;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly ITokenStorageService _tokenStorageService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly AuthenticationOptions _options;
    private OidcClient? _oidcClient;
    
    public event EventHandler<bool>? AuthenticationChanged;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public ClaimsPrincipal User { get; private set; } = new ClaimsPrincipal(new ClaimsIdentity());

    public AuthenticationService(
        ITokenStorageService tokenStorageService, 
        ILogger<AuthenticationService> logger,
        IOptions<AuthenticationOptions> options)
    {
        _tokenStorageService = tokenStorageService;
        _logger = logger;
        _options = options.Value;
        InitializeOidcClient();
    }

    [System.Diagnostics.CodeAnalysis.MemberNotNull(nameof(_oidcClient))]
    private void InitializeOidcClient()
    {
        var options = new OidcClientOptions
        {
            Authority = _options.Authority,
            ClientId = _options.ClientId,
            RedirectUri = _options.RedirectUri,
            Scope = _options.Scope,
            Browser = new SystemBrowser(), // Required: We need a system browser implementation
            Policy = new Policy
            {
                 RequireAccessTokenHash = false // Sometimes needed depending on IdP
            } 
        };

        _oidcClient = new OidcClient(options);
    }

    public async Task InitializeAsync()
    {
        var (accessToken, refreshToken, idToken) = await _tokenStorageService.GetTokensAsync();

        if (!string.IsNullOrEmpty(refreshToken))
        {
            // Try to refresh the token
            await RefreshTokenAsync(refreshToken);
        }
        else if (!string.IsNullOrEmpty(accessToken)) 
        {
             // If we only have access token (unlikely with our flow but possible), we might validate it.
             // For now, simpler to require refresh token for persistence or just login again.
             // We can check if it's expired.
             // A better approach for simple check is below:
             
             // TODO: Validate existing access token or assume logged out if no refresh token
        }
    }

    public async Task<bool> LoginAsync()
    {
        try
        {
            var loginResult = await _oidcClient!.LoginAsync(new LoginRequest());

            if (loginResult.IsError)
            {
                _logger.LogError("Login Error: {Error}", loginResult.Error);
                return false;
            }

            // Save tokens
            await _tokenStorageService.SaveTokensAsync(loginResult.AccessToken, loginResult.RefreshToken, loginResult.IdentityToken);

            _logger.LogInformation("Login Successful");
            _logger.LogInformation("Access Token: {AccessToken}", loginResult.AccessToken);
            _logger.LogInformation("Refresh Token: {RefreshToken}", loginResult.RefreshToken);
            _logger.LogInformation("Identity Token: {IdentityToken}", loginResult.IdentityToken);

            // Set User
            User = loginResult.User;
            AuthenticationChanged?.Invoke(this, true);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login Exception");
            return false;
        }
    }

    private async Task RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var result = await _oidcClient!.RefreshTokenAsync(refreshToken);

            if (result.IsError)
            {
                 // Token refresh failed (expired or revoked), clear tokens
                 _logger.LogWarning("Token refresh failed: {Error}", result.Error);
                 await LogoutAsync();
                 return;
            }

            // Update tokens
            await _tokenStorageService.SaveTokensAsync(result.AccessToken, result.RefreshToken, result.IdentityToken);

            _logger.LogInformation("Token Refresh Successful");
            _logger.LogInformation("New Access Token: {AccessToken}", result.AccessToken);

            // Update User
            // Since RefreshTokenResult doesn't return the User implementation, we fetch fresh info.
            var userInfoResult = await _oidcClient.GetUserInfoAsync(result.AccessToken);

            if (userInfoResult.IsError)
            {
                _logger.LogError("UserInfo Error during refresh: {Error}", userInfoResult.Error);
                await LogoutAsync();
                return;
            }

            var identity = new ClaimsIdentity(userInfoResult.Claims, "OIDC", "name", "role");
            User = new ClaimsPrincipal(identity);
            
            AuthenticationChanged?.Invoke(this, true);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "RefreshToken Exception");
            await LogoutAsync();
        }
    }

    public async Task LogoutAsync()
    {
        try 
        {
            // Optional: Call IdP logout if needed
            // await _oidcClient.LogoutAsync(new LogoutRequest { IdTokenHint = ... });
            
            await _tokenStorageService.ClearTokensAsync();
            User = new ClaimsPrincipal(new ClaimsIdentity());
            AuthenticationChanged?.Invoke(this, false);
        }
        catch(Exception)
        {
            // Ignore logout errors
        }
    }
}
