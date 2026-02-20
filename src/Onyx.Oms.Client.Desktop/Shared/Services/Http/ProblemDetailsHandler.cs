using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Models;

namespace Onyx.Oms.Client.Desktop.Shared.Services.Http;

public class ProblemDetailsHandler : DelegatingHandler
{
    private readonly IToastService _toastService;
    private readonly IDialogService _dialogService;
    private readonly ILogger<ProblemDetailsHandler> _logger;

    public ProblemDetailsHandler(
        IToastService toastService, 
        IDialogService dialogService,
        ILogger<ProblemDetailsHandler> logger)
    {
        _toastService = toastService;
        _dialogService = dialogService;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return response;
            }

            // Handle Error Responses
            _logger.LogWarning("API Error: {StatusCode} {ReasonPhrase} for {Method} {Uri}", 
                response.StatusCode, response.ReasonPhrase, request.Method, request.RequestUri);
            
            await HandleErrorResponse(response);
            
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network Error for {Method} {Uri}", request.Method, request.RequestUri);
            _toastService.ShowError("Network Error", "Unable to connect to the server. Please check your internet connection.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected Error for {Method} {Uri}", request.Method, request.RequestUri);
            _toastService.ShowError("Unexpected Error", ex.Message);
            throw;
        }
    }

    private async Task HandleErrorResponse(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            
            // Log raw content for debugging
            _logger.LogWarning("Error Content: {Content}", content);

            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                _toastService.ShowError("Access Denied", "You do not have permission to perform this action.");
                return;
            }

            if (response.StatusCode >= HttpStatusCode.InternalServerError)
            {
                 _toastService.ShowError("Server Error", "Something went wrong on the server. Please try again later.");
                 return;
            }

            // Try to parse ProblemDetails
            if (!string.IsNullOrWhiteSpace(content))
            {
                var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (problemDetails != null)
                {
                    var title = problemDetails.Title ?? "Error";
                    var detail = problemDetails.Detail ?? "An error occurred."; // Capture detail for logging
                    
                    // The errors array can be at the root, or nested in extensions depending on the backend format
                    var errors = problemDetails.Errors ?? problemDetails.Extensions?.Errors;

                    _logger.LogWarning("ProblemDetails: {Title} - {Detail}", title, detail);

                    if (errors != null && errors.Count > 1)
                    {
                        // Multiple Validation Errors -> Dialog
                        var errorDescriptions = errors.Select(e => e.Description ?? "Unknown error");
                        await _dialogService.ShowValidationErrorsAsync(title, errorDescriptions);
                    }
                    else if (errors != null && errors.Count == 1)
                    {
                        // Single Error -> Toast
                        var error = errors.First();
                        _toastService.ShowError(title, error.Description ?? "Unknown error");
                    }
                    else
                    {
                        // No specific errors list -> Toast the title/detail
                        _toastService.ShowError(title, detail);
                    }
                    return;
                }
            }
            
            // Fallback for non-JSON errors
            _toastService.ShowError($"Error {response.StatusCode}", response.ReasonPhrase ?? "Unknown error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle error response parsing");
            // Failed to parse or handle -> Generic Toast
            _toastService.ShowError("Error", "An unexpected error occurred while processing the response.");
        }
    }
}
