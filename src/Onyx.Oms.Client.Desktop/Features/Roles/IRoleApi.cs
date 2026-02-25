using Refit;
using System;
using System.Threading.Tasks;
using Onyx.Oms.Client.Desktop.Shared.Models;
using System.Collections.Generic;

namespace Onyx.Oms.Client.Desktop.Features.Roles;

public interface IRoleApi
{
    [Get("/api/v1/permissions")]
    Task<List<PermissionGroupDto>> GetAllPermissions();

    [Get("/api/v1/roles/search")]
    Task<PagedResult<RoleDto>> SearchRoles(
        [AliasAs("Page")] int page,
        [AliasAs("PageSize")] int pageSize,
        [AliasAs("SearchTerm")] string? searchTerm = null,
        [AliasAs("SortColumn")] string? sortColumn = null,
        [AliasAs("SortOrder")] string? sortOrder = null,
        [AliasAs("IsActive")] bool? isActive = null);

    [Get("/api/v1/roles/{id}")]
    Task<RoleWithPermissionsDto> GetRoleById(Guid id); 

    [Post("/api/v1/roles")]
    Task<Guid> CreateRole([Body] CreateRoleDto role);

    [Put("/api/v1/roles/{id}")]
    Task UpdateRole(Guid id, [Body] UpdateRoleDto role);

    [Put("/api/v1/roles/{id}/activate")]
    Task ActivateRole(Guid id);

    [Put("/api/v1/roles/{id}/deactivate")]
    Task DeactivateRole(Guid id);

    [Delete("/api/v1/roles/{id}")]
    Task DeleteRole(Guid id);
}
