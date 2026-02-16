using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Onyx.Oms.Client.Desktop.Shared.Shell;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class ActivationService : IActivationService
{
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly INavigationService _navigationService;
    private readonly IServiceProvider _serviceProvider;
    private Window? _mainWindow;

    public ActivationService(
        ActivationHandler<LaunchActivatedEventArgs> defaultHandler, 
        IEnumerable<IActivationHandler> activationHandlers, 
        INavigationService navigationService,
        IServiceProvider serviceProvider)
    {
        _defaultHandler = defaultHandler;
        _activationHandlers = activationHandlers;
        _navigationService = navigationService;
        _serviceProvider = serviceProvider;
    }

    public async Task ActivateAsync(object activationArgs)
    {
        // 1. Initialize Window
        if (_mainWindow == null)
        {
             _mainWindow = _serviceProvider.GetService(typeof(MainWindow)) as Window;
        }

        if (_mainWindow != null)
        {
            // 2. Activate Window
            _mainWindow.Activate();

            // 3. Handle activation via NavigationService
            // Note: We need to ensure MainWindow's Frame is linked to NavigationService
            if (_mainWindow is MainWindow shellWindow)
            {
                 // We need to expose the Frame from MainWindow or set it here
                 // For now, let's assume we can get it or MainWindow registers itself.
                 // A common pattern is MainWindow.xaml.cs sets NavigationService.Frame = ContentFrame;
            }
        }


        // 4. Handle Activation Arguments
        await HandleActivationAsync(activationArgs);
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
        var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

        if (activationHandler != null)
        {
            await activationHandler.HandleAsync(activationArgs);
        }

        if (_defaultHandler.CanHandle(activationArgs))
        {
            await _defaultHandler.HandleAsync(activationArgs);
        }
    }
}
