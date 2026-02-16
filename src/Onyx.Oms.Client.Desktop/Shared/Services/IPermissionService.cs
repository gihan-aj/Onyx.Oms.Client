using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public interface IPermissionService
{
    Task<bool> CanNavigateToAsync(string pageKey);
    Task<bool> CanExecuteAsync(string permissionKey);
}
