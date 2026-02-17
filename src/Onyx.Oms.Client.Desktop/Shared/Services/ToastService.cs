using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class ToastService : IToastService
{
    private InfoBar? _infoBar;

    private Microsoft.UI.Dispatching.DispatcherQueueTimer? _timer;

    public void RegisterInfoBar(InfoBar infoBar)
    {
        _infoBar = infoBar;
    }

    public void ShowSuccess(string title, string message, int durationInSeconds = 5) => Show(title, message, durationInSeconds, InfoBarSeverity.Success);
    public void ShowError(string title, string message, int durationInSeconds = 8) => Show(title, message, durationInSeconds, InfoBarSeverity.Error);
    public void ShowInfo(string title, string message, int durationInSeconds = 5) => Show(title, message, durationInSeconds, InfoBarSeverity.Informational);
    public void ShowWarning(string title, string message, int durationInSeconds = 8) => Show(title, message, durationInSeconds, InfoBarSeverity.Warning);

    private void Show(string title, string message, int durationInSeconds, InfoBarSeverity severity)
    {
        if (_infoBar == null) return;

        _infoBar.DispatcherQueue.TryEnqueue(() =>
        {
            _infoBar.Title = title;
            _infoBar.Message = message;
            _infoBar.Severity = severity;
            _infoBar.IsOpen = true;

            StartTimer(durationInSeconds);
        });
    }

    private void StartTimer(int duration = 5)
    {
        StopTimer();
        if (_infoBar == null) return;

        _timer = _infoBar.DispatcherQueue.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(duration);
        _timer.Tick += (s, e) =>
        {
            if (_infoBar != null) _infoBar.IsOpen = false;
            StopTimer();
        };
        _timer.Start();
    }

    private void StopTimer()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer = null;
        }
    }
}
