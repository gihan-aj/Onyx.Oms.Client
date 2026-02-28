using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services.Http;
using System;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class TenantProfileService : ITenantProfileService
{
    private readonly ITenantProfileApi _tenantProfileApi;
    private readonly ILogger<TenantProfileService> _logger;
    
    public TenantProfileDto? Profile { get; private set; }

    public TenantProfileService(ITenantProfileApi tenantProfileApi, ILogger<TenantProfileService> logger)
    {
        _tenantProfileApi = tenantProfileApi;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Fetching tenant profile...");
            Profile = await _tenantProfileApi.GetTenantProfile();
            _logger.LogInformation("Loaded tenant profile for {StoreName}.", Profile?.StoreName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load tenant profile.");
            Profile = null;
        }
    }

    public void ClearProfile()
    {
        Profile = null;
        _logger.LogInformation("Tenant profile cleared due to logout.");
    }
}
