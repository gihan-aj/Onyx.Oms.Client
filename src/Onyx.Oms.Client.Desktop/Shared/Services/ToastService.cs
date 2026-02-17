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

    public void ShowSuccess(string title, string message) => Show(title, message, InfoBarSeverity.Success);
    public void ShowError(string title, string message) => Show(title, message, InfoBarSeverity.Error);
    public void ShowInfo(string title, string message) => Show(title, message, InfoBarSeverity.Informational);
    public void ShowWarning(string title, string message) => Show(title, message, InfoBarSeverity.Warning);

    private void Show(string title, string message, InfoBarSeverity severity)
    {
        if (_infoBar == null) return;

        _infoBar.DispatcherQueue.TryEnqueue(() =>
        {
            _infoBar.Title = title;
            _infoBar.Message = message;
            _infoBar.Severity = severity;
            _infoBar.IsOpen = true;

            StartTimer();
        });
    }

    private void StartTimer()
    {
        StopTimer();
        if (_infoBar == null) return;

        _timer = _infoBar.DispatcherQueue.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(3);
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
