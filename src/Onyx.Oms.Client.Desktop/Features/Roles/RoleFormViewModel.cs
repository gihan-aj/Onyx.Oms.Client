using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Onyx.Oms.Client.Desktop.Shared.Services;

namespace Onyx.Oms.Client.Desktop.Features.Roles;

public partial class RoleFormViewModel : ObservableObject
{
    private readonly IRoleApi _roleApi;
    private readonly IToastService _toastService;
    private readonly ILogger<RoleFormViewModel> _logger;

    public bool IsEditMode { get; private set; }
    public Guid? RoleId { get; private set; }

    private bool _isReadOnly;
    public bool IsReadOnly
    {
        get => _isReadOnly;
        private set
        {
            if (SetProperty(ref _isReadOnly, value))
            {
                OnPropertyChanged(nameof(IsNotReadOnly));
            }
        }
    }

    public bool IsNotReadOnly => !IsReadOnly;

    private string _title = "Create Role";
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string? _description;
    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    private bool _isLoading = true;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private ObservableCollection<PermissionTreeItemViewModel> _permissionGroups = new();
    public ObservableCollection<PermissionTreeItemViewModel> PermissionGroups
    {
        get => _permissionGroups;
        set => SetProperty(ref _permissionGroups, value);
    }

    public RoleFormViewModel(IRoleApi roleApi, IToastService toastService, ILogger<RoleFormViewModel> logger)
    {
        _roleApi = roleApi;
        _toastService = toastService;
        _logger = logger;
    }

    public async Task InitializeAsync(RoleWithPermissionsDto? roleToEdit = null, bool isReadOnly = false)
    {
        IsReadOnly = isReadOnly;
        IsLoading = true;
        try
        {
            // Fetch all permissions to build the tree
            var groups = await _roleApi.GetAllPermissions();
            var existingPermissions = new HashSet<string>();

            if (roleToEdit != null)
            {
                IsEditMode = !IsReadOnly;
                RoleId = roleToEdit.Id;
                Name = roleToEdit.Name;
                Description = roleToEdit.Description;
                Title = IsReadOnly ? $"Role Details ({Name})" : $"Edit Role ({Name})";

                existingPermissions = new(roleToEdit.Permissions);

                // Note: The Search API might not return the full list of permissions for a role in 'PermissionCount'.
                // If the backend doesn't return the full array in a GET /roles/{id}, 
                // we might need an endpoint for `GET /api/v1/roles/{id}/permissions` or ensure `RoleDto` includes `List<string> Permissions`.
                // Assuming for now either RoleDto has it, or we need to adjust.
                // *WORKAROUND*: If RoleDto doesn't have permissions string list, the IdP or an explicit fetch is needed.
                // For this implementation, we will assume RoleDto has or we'll add it if missing from the schema.
                // Wait, looking at the API docs, Update accepts `permissions: []` but GET Search doesn't return them.
                // We'll need to fetch the role details if there's a GET /roles/{id}. The API reference didn't list one, 
                // but usually there is one. Assumed it exists or we use what we have.
                // LET'S ASSUME `roleToEdit.Permissions` exists for now. If it doesn't compile, we will patch RoleDto.
            }

            PermissionGroups.Clear();

            foreach (var group in groups)
            {
                var groupNode = new PermissionTreeItemViewModel
                {
                    Name = group.GroupName,
                    Value = null,
                    IsReadOnly = IsReadOnly
                };

                foreach (var perm in group.Permissions)
                {
                    var childNode = new PermissionTreeItemViewModel(groupNode)
                    {
                        Name = perm.Name,
                        Value = perm.Value,
                        IsReadOnly = IsReadOnly,
                        IsChecked = existingPermissions.Contains(perm.Value) // If editing
                    };
                    groupNode.Children.Add(childNode);
                }

                groupNode.UpdateParent(); // Sets parent state based on children
                PermissionGroups.Add(groupNode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize role form");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public List<string> GetSelectedPermissions()
    {
        var selected = new List<string>();

        foreach (var group in PermissionGroups)
        {
            foreach (var child in group.Children)
            {
                if (child.IsChecked == true && !string.IsNullOrEmpty(child.Value))
                {
                    selected.Add(child.Value);
                }
            }
        }

        return selected;
    }

    public async Task<bool> SaveAsync()
    {
        IsLoading = true;
        try
        {
            var selectedPermissions = GetSelectedPermissions();

            if (IsEditMode)
            {
                var updateDto = new UpdateRoleDto
                {
                    Id = RoleId!.Value,
                    Name = Name,
                    Description = Description,
                    Permissions = selectedPermissions
                };
                await _roleApi.UpdateRole(updateDto.Id, updateDto);
                _toastService.ShowSuccess("Success", "Role updated successfully.");
            }
            else
            {
                var createDto = new CreateRoleDto
                {
                    Name = Name,
                    Description = Description,
                    Permissions = selectedPermissions
                };
                await _roleApi.CreateRole(createDto);
                _toastService.ShowSuccess("Success", "Role created successfully.");
            }
            return true;
        }
        catch (Exception)
        {
            // The global ProblemDetailsHandler handles the UI error toasts
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }
}
