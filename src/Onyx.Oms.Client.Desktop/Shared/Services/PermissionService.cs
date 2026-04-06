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

    public async Task<bool> InitializeAsync()
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
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load user permissions.");
            _permissions.Clear();
            return false;
        }
    }

    public void ClearPermissions()
    {
        _permissions.Clear();
        _logger.LogInformation("Permissions cleared due to logout.");
    }

    public bool CanExecute(string permissionKey)
    {
        return _permissions.Contains(permissionKey);
    }

    public bool HasFeatureAccess(string featurePrefix)
    {
        // True if the user has any permission starting with the given prefix
        foreach (var perm in _permissions)
        {
            if (perm.StartsWith(featurePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    public bool CanNavigateTo(string pageKey)
    {
        if (pageKey == typeof(Features.Roles.RolesPage).FullName ||
            pageKey == typeof(Features.Roles.RoleFormPage).FullName)
        {
            return HasFeatureAccess("tenant:roles:");
        }
        
        if (pageKey == typeof(Features.Couriers.CouriersPage).FullName)
        {
            return HasFeatureAccess("tenant:couriers:");
        }

        // Add other restricted pages here

        // Dashboard, Settings, etc. are allowed by default if logged in
        return true;
    }
}
