using System;
using System.Collections.Generic;

namespace Onyx.Oms.Client.Desktop.Features.Roles;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PermissionCount { get; set; }
    public int UserCount { get; set; }
    public bool IsActive { get; set; }
    
    // UI Permission Flags
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanActivate { get; set; }
    public bool CanDeactivate { get; set; }
}

public class RoleWithPermissionsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class UpdateRoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
}


