using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class PermissionService : IPermissionService
{
    public Task<bool> CanExecuteAsync(string permissionKey)
    {
        // TODO: Implement actual permission logic
        return Task.FromResult(true);
    }

    public Task<bool> CanNavigateToAsync(string pageKey)
    {
        // TODO: Implement actual route guard logic
        return Task.FromResult(true);
    }
}
