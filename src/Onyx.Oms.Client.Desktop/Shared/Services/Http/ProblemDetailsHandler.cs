using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Onyx.Oms.Client.Desktop.Shared.Models;

namespace Onyx.Oms.Client.Desktop.Shared.Services.Http;

public class ProblemDetailsHandler : DelegatingHandler
{
    private readonly IToastService _toastService;
    private readonly IDialogService _dialogService;

    public ProblemDetailsHandler(IToastService toastService, IDialogService dialogService)
    {
        _toastService = toastService;
        _dialogService = dialogService;
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
            await HandleErrorResponse(response);
            
            return response;
        }
        catch (HttpRequestException ex)
        {
            _toastService.ShowError("Network Error", "Unable to connect to the server. Please check your internet connection.");
            throw;
        }
        catch (Exception ex)
        {
            _toastService.ShowError("Unexpected Error", ex.Message);
            throw;
        }
    }

    private async Task HandleErrorResponse(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();

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
                    var errors = problemDetails.Extensions?.Errors;

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
                        _toastService.ShowError(title, problemDetails.Detail ?? "An error occurred.");
                    }
                    return;
                }
            }
            
            // Fallback for non-JSON errors
            _toastService.ShowError($"Error {response.StatusCode}", response.ReasonPhrase ?? "Unknown error");
        }
        catch (Exception)
        {
            // Failed to parse or handle -> Generic Toast
            _toastService.ShowError("Error", "An unexpected error occurred while processing the response.");
        }
    }
}
