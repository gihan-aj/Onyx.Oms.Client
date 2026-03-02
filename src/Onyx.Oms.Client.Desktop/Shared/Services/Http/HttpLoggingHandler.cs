using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services.Http;
public class HttpLoggingHandler : DelegatingHandler
{
    private readonly ILogger<HttpLoggingHandler> _logger;

    public HttpLoggingHandler(ILogger<HttpLoggingHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Log Request
        _logger.LogDebug("=== HTTP Request ===");
        _logger.LogDebug("{Method} {Uri}", request.Method, request.RequestUri);
        if (request.Content is not null)
        {
            var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Request Body: {Body}", requestBody);
        }
        var response = await base.SendAsync(request, cancellationToken);
        // Log Response
        _logger.LogDebug("=== HTTP Response ===");
        _logger.LogDebug("Status: {StatusCode}", response.StatusCode);
        if (response.Content is not null)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Response Body: {Body}", responseBody);
        }
        return response;
    }
}
