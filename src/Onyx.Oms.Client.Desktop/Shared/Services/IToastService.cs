using System;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public interface IToastService
{
    void ShowSuccess(string title, string message, int durationInSeconds = 5);
    void ShowError(string title, string message, int durationInSeconds = 8);
    void ShowInfo(string title, string message, int durationInSeconds = 5);
    void ShowWarning(string title, string message, int durationInSeconds = 8);
}
