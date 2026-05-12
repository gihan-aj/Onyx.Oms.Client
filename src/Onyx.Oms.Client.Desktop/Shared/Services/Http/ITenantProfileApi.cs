using Onyx.Oms.Client.Desktop.Shared.Models;
using Refit;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services.Http;

public interface ITenantProfileApi
{
    [Get("/api/v1/settings/profile")]
    Task<TenantProfileDto> GetTenantProfile();

    [Put("/api/v1/settings/profile/store-info")]
    Task UpdateStoreInfo([Body] UpdateStoreInfoDto dto);

    [Put("/api/v1/settings/profile/regional-settings")]
    Task UpdateRegionalSettings([Body] UpdateRegionalSettingsDto dto);

    [Put("/api/v1/settings/profile/address")]
    Task UpdateStoreAddress([Body] UpdateStoreAddressDto dto);

    [Put("/api/v1/settings/profile/preferences")]
    Task UpdatePreferences([Body] UpdatePreferencesDto dto);
}
