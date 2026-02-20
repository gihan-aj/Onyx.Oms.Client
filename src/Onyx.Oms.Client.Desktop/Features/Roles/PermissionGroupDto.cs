using System.Collections.Generic;

namespace Onyx.Oms.Client.Desktop.Features.Roles;

public class PermissionDto
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class PermissionGroupDto
{
    public string GroupName { get; set; } = string.Empty;
    public List<PermissionDto> Permissions { get; set; } = new();
}
