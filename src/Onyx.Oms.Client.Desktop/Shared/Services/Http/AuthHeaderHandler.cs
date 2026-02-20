using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services.Http;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly IAuthenticationService _authenticationService;

    public AuthHeaderHandler(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // In a real app, you would retrieve the Access Token (string) from IAuthenticationService
        // For now, we assume the mock service implies a token or we just add a dummy one if authenticated
        // The mock IAuthenticationService currently exposes a ClaimsPrincipal but not a raw token string property.
        // We will skip adding the header if not authenticated or if we can't get a token.
        
        if (_authenticationService.IsAuthenticated && !string.IsNullOrEmpty(_authenticationService.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authenticationService.AccessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
