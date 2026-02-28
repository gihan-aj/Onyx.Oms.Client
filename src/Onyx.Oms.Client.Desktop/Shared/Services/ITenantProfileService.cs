using Onyx.Oms.Client.Desktop.Shared.Models;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public interface ITenantProfileService
{
    TenantProfileDto? Profile { get; }
    
    Task InitializeAsync();
    
    void ClearProfile();
}
