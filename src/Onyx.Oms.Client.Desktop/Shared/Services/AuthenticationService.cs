using Duende.IdentityModel.OidcClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Onyx.Oms.Client.Desktop.Shared.Models.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
    public event EventHandler<bool>? AuthenticationProcessStateChanged;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string? AccessToken { get; private set; }

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
             AccessToken = accessToken;
             // If we only have access token (unlikely with our flow but possible), we might validate it.
             // For now, simpler to require refresh token for persistence or just login again.
             // We can check if it's expired.
             // A better approach for simple check is below:
             
             // TODO: Validate existing access token or assume logged out if no refresh token
        }
    }

    public Task<CurrentUser?> GetCurrentUserAsync()
    {
        if (!IsAuthenticated || string.IsNullOrEmpty(AccessToken))
        {
            return Task.FromResult<CurrentUser?>(null);
        }

        // The default OidcClient User may only contain a subset of claims from the id_token.
        // We will parse the Access Token directly to get all enriched claims (like roles, name).
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(AccessToken))
        {
            return Task.FromResult<CurrentUser?>(null);
        }

        var jwtToken = handler.ReadJwtToken(AccessToken);
        var claims = jwtToken.Claims;

        var id = claims.FirstOrDefault(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)?.Value;
        var firstName = claims.FirstOrDefault(c => c.Type == "given_name" || c.Type == ClaimTypes.GivenName)?.Value;
        var lastName = claims.FirstOrDefault(c => c.Type == "family_name" || c.Type == ClaimTypes.Surname)?.Value;
        var email = claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email)?.Value;

        var roles = new System.Collections.Generic.List<string>();
        foreach (var claim in claims.Where(c => c.Type == "role" || c.Type == ClaimTypes.Role))
        {
            roles.Add(claim.Value);
        }

        return Task.FromResult<CurrentUser?>(new CurrentUser(id, firstName, lastName, email, roles.ToArray()));
    }

    public async Task<bool> LoginAsync()
    {
        try
        {
            AuthenticationProcessStateChanged?.Invoke(this, true);
            var loginResult = await _oidcClient!.LoginAsync(new LoginRequest());

            if (loginResult.IsError)
            {
                _logger.LogError("Login Error: {Error}", loginResult.Error);
                AuthenticationProcessStateChanged?.Invoke(this, false);
                return false;
            }

            // Save tokens
            await _tokenStorageService.SaveTokensAsync(loginResult.AccessToken, loginResult.RefreshToken, loginResult.IdentityToken);

            AccessToken = loginResult.AccessToken;


            _logger.LogInformation("Login Successful");
            _logger.LogInformation("Access Token: {AccessToken}", loginResult.AccessToken);
            _logger.LogInformation("Refresh Token: {RefreshToken}", loginResult.RefreshToken);
            _logger.LogInformation("Identity Token: {IdentityToken}", loginResult.IdentityToken);

            // Set User
            User = loginResult.User;
            AuthenticationProcessStateChanged?.Invoke(this, false);
            AuthenticationChanged?.Invoke(this, true);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login Exception");
            AuthenticationProcessStateChanged?.Invoke(this, false);
            return false;
        }
    }

    private async Task RefreshTokenAsync(string refreshToken)
    {
        try
        {
            AuthenticationProcessStateChanged?.Invoke(this, true);
            var result = await _oidcClient!.RefreshTokenAsync(refreshToken);

            if (result.IsError)
            {
                 // Token refresh failed (expired or revoked), clear tokens
                 _logger.LogWarning("Token refresh failed: {Error}", result.Error);
                 AuthenticationProcessStateChanged?.Invoke(this, false);
                 await LogoutAsync();
                 return;
            }

            // Update tokens
            await _tokenStorageService.SaveTokensAsync(result.AccessToken, result.RefreshToken, result.IdentityToken);

            AccessToken = result.AccessToken;

            _logger.LogInformation("Token Refresh Successful");
            _logger.LogInformation("New Access Token: {AccessToken}", result.AccessToken);

            // Update User
            // Since RefreshTokenResult doesn't return the User implementation, we fetch fresh info.
            var userInfoResult = await _oidcClient.GetUserInfoAsync(result.AccessToken);

            if (userInfoResult.IsError)
            {
                _logger.LogError("UserInfo Error during refresh: {Error}", userInfoResult.Error);
                AuthenticationProcessStateChanged?.Invoke(this, false);
                await LogoutAsync();
                return;
            }

            var identity = new ClaimsIdentity(userInfoResult.Claims, "OIDC", "name", "role");
            User = new ClaimsPrincipal(identity);
            
            AuthenticationProcessStateChanged?.Invoke(this, false);
            AuthenticationChanged?.Invoke(this, true);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "RefreshToken Exception");
            AuthenticationProcessStateChanged?.Invoke(this, false);
            await LogoutAsync();
        }
    }

    public async Task LogoutAsync()
    {
        try 
        {
            var (_, _, idToken) = await _tokenStorageService.GetTokensAsync();
            if (_oidcClient != null)
            {
                await _oidcClient.LogoutAsync(new LogoutRequest { IdTokenHint = idToken });
            }
            
            await _tokenStorageService.ClearTokensAsync();
            User = new ClaimsPrincipal(new ClaimsIdentity());
            AccessToken = null;
            AuthenticationChanged?.Invoke(this, false);
        }
        catch(Exception)
        {
            // Ignore logout errors
        }
    }
}
