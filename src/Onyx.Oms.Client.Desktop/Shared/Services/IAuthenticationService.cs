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
    string? AccessToken { get; }
}
