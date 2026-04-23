using System.Collections.Generic;
using System.Net;
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
        string? currentToken = null;
        if (_authenticationService.IsAuthenticated && !string.IsNullOrEmpty(_authenticationService.AccessToken))
        {
            currentToken = _authenticationService.AccessToken;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", currentToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
        {
            bool refreshed = await _authenticationService.RefreshTokenAsync(currentToken);
            
            if (refreshed && !string.IsNullOrEmpty(_authenticationService.AccessToken))
            {
                var newRequest = await CloneRequestAsync(request);
                newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authenticationService.AccessToken);
                
                response.Dispose();
                
                return await base.SendAsync(newRequest, cancellationToken);
            }
        }

        return response;
    }

    private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        if (request.Content != null)
        {
            var ms = new System.IO.MemoryStream();
            await request.Content.CopyToAsync(ms).ConfigureAwait(false);
            ms.Position = 0;
            clone.Content = new StreamContent(ms);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.Add(header.Key, header.Value);
            }
        }

        foreach (KeyValuePair<string, object?> option in request.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        }

        foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
