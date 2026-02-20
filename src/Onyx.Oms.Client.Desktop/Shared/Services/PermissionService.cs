using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Shared.Services.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class PermissionService : IPermissionService
{
    private readonly IUserApi _userApi;
    private readonly ILogger<PermissionService> _logger;
    private HashSet<string> _permissions = new(StringComparer.OrdinalIgnoreCase);

    public PermissionService(IUserApi userApi, ILogger<PermissionService> logger)
    {
        _userApi = userApi;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Fetching user permissions...");
            var permissionsList = await _userApi.GetUserPermissionsAsync();
            
            _permissions.Clear();
            foreach (var permission in permissionsList)
            {
                _permissions.Add(permission);
            }
            
            _logger.LogInformation("Loaded {Count} permissions.", _permissions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load user permissions.");
            _permissions.Clear();
        }
    }

    public void ClearPermissions()
    {
        _permissions.Clear();
        _logger.LogInformation("Permissions cleared due to logout.");
    }

    public Task<bool> CanExecuteAsync(string permissionKey)
    {
        // If not empty, check if we have the specific permission.
        return Task.FromResult(_permissions.Contains(permissionKey));
    }

    public Task<bool> CanNavigateToAsync(string pageKey)
    {
        // For now, allow navigation to all pages. 
        // We can hook this up to a mapping of Page => RequiredPermission later.
        return Task.FromResult(true);
    }
}
