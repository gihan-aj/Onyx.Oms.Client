using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Onyx.Oms.Client.Desktop.Features.Dashboard;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
{
    private readonly INavigationService _navigationService;

    public DefaultActivationHandler(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
        // None of the activation handlers have handled the app activation
        return _navigationService.Frame?.Content == null;
    }

    protected override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
        _navigationService.NavigateTo(typeof(DashboardPage).FullName!, args.Arguments, clearNavigation: true);
        await Task.CompletedTask;
    }
}
