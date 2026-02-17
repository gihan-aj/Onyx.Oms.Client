using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public interface IDialogService
{
    Task ShowMessageAsync(string title, string message);
    Task ShowErrorAsync(string title, string message);
    Task ShowValidationErrorsAsync(string title, IEnumerable<string> errors);
    Task<bool> ShowConfirmationAsync(string title, string message, string yesText = "Yes", string noText = "No");
}
