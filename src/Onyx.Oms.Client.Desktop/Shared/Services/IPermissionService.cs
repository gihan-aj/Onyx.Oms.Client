using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public interface IPermissionService
{
    bool CanNavigateTo(string pageKey);
    bool CanExecute(string permissionKey);
    bool HasFeatureAccess(string featurePrefix);
    Task InitializeAsync();
    void ClearPermissions();
}
