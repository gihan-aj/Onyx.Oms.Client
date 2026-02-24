using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Onyx.Oms.Client.Desktop.Features.Roles;

public class PermissionDisplayGroup
{
    public string GroupName { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
}

public sealed partial class RoleDetailsDialog : ContentDialog
{
    public RoleWithPermissionsDto Role { get; }
    public List<PermissionDisplayGroup> GroupedPermissions { get; }

    public RoleDetailsDialog(RoleWithPermissionsDto role)
    {
        Role = role;
        GroupedPermissions = GroupPermissions(role.Permissions ?? new List<string>());
        InitializeComponent();
    }

    private List<PermissionDisplayGroup> GroupPermissions(List<string> rawPermissions)
    {
        var groups = new Dictionary<string, List<string>>();
        foreach (var perm in rawPermissions)
        {
            var parts = perm.Split('.');
            string groupName = "Other";
            string actionName = perm;

            if (parts.Length >= 3 && parts[0] == "Permissions")
            {
                groupName = parts[1];
                actionName = string.Join(".", parts.Skip(2));
            }
            else if (parts.Length == 2 && parts[0] == "Permissions")
            {
                groupName = parts[1];
                actionName = parts[1];
            }
            
            if (!groups.ContainsKey(groupName))
            {
                groups[groupName] = new List<string>();
            }
            groups[groupName].Add(actionName);
        }

        return groups.Select(g => new PermissionDisplayGroup 
        { 
            GroupName = g.Key, 
            Permissions = g.Value.OrderBy(p => p).ToList() 
        }).OrderBy(g => g.GroupName).ToList();
    }
}
