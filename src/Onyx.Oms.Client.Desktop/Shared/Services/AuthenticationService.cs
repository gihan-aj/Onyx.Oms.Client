using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class AuthenticationService : IAuthenticationService
{
    public event EventHandler<bool>? AuthenticationChanged;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public ClaimsPrincipal User { get; private set; } = new ClaimsPrincipal(new ClaimsIdentity());

    public async Task<bool> LoginAsync()
    {
        // Mock Login - Simulate delay and return true
        await Task.Delay(500);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "John Doe"),
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim(ClaimTypes.Surname, "Doe"),
            new Claim(ClaimTypes.Email, "john.doe@onyx.com"),
            new Claim("sub", "1234567890")
        };

        var identity = new ClaimsIdentity(claims, "MockAuth");
        User = new ClaimsPrincipal(identity);

        AuthenticationChanged?.Invoke(this, true);
        return true;
    }

    public async Task LogoutAsync()
    {
        // Mock Logout
        await Task.Delay(500);

        User = new ClaimsPrincipal(new ClaimsIdentity());
        AuthenticationChanged?.Invoke(this, false);
    }
}
