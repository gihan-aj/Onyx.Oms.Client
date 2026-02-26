using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public interface IAuthenticationService
{
    event EventHandler<bool>? AuthenticationChanged;
    bool IsAuthenticated { get; }
    ClaimsPrincipal User { get; }
    Task<bool> LoginAsync();
    Task LogoutAsync();
    Task InitializeAsync();
    Task<CurrentUser?> GetCurrentUserAsync();
    string? AccessToken { get; }
}

public record CurrentUser(string? Id, string? FirstName, string? LastName, string? Email, string[] Roles);
