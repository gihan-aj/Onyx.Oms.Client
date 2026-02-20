# Roles API Reference

This document outlines the API endpoints available for managing Roles in the Onyx.Oms system. You can use this reference to implement the UI features in the Desktop (WinUI) application.

All endpoints require authentication (Bearer token) and specific system permissions.

## Base URL
`api/v1/roles`

---

## 1. Get Roles (Paged)

Retrieves a paginated list of roles, with optional searching, sorting, and filtering by activity status.

* **Endpoint**: `GET /api/v1/roles/search`
* **Permission Required**: `Permissions.Roles.View`

### Query Parameters
| Parameter | Type | Required | Default | Description |
| :--- | :--- | :--- | :--- | :--- |
| `Page` | `int` | No | `1` | The page number to retrieve. |
| `PageSize` | `int` | No | `10` | Number of items per page. |
| `SearchTerm` | `string` | No | `null` | Wildcard search term (searches by Role `Name`). |
| `SortColumn` | `string` | No | `null` | The property to sort by (e.g., "Name", "PermissionCount", "UserCount", "IsActive"). |
| `SortOrder` | `string` | No | `null` | The sort direction (`"asc"` or `"desc"`). |
| `IsActive` | `bool` | No | `null` | Filter by active status (`true` for active, `false` for inactive roles). If omitted, returns all. |

### Response Details
The response will be a wrapped standard Result object containing a `PagedResult<RoleDto>`.

**Response Body (`Value` property contents):**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Refund Clerk",
      "description": "Handles refunds.",
      "permissionCount": 5,
      "userCount": 2,
      "isActive": true
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 25,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

## 2. Create Role

Creates a new role locally and synchronizes the role name with the external Identity Provider (IdP).

* **Endpoint**: `POST /api/v1/roles`
* **Permission Required**: `Permissions.Roles.Create`

### Request Body
```json
{
  "name": "Warehouse Manager",
  "description": "Able to manage stock",
  "permissions": [
    "Permissions.Products.View",
    "Permissions.Products.Create"
  ]
}
```

### Response
Returns a Standard Result containing the new Role's `Guid` ID.
```json
{
  "isSuccess": true,
  "value": "11b712fa-3e6f-4d33-91db-de5afdf8f5bc"
}
```

---

## 3. Update Role

Updates the name, description, and permissions of a role. If the name is changed, the backend synchronizes the rename with the IdP.

* **Endpoint**: `PUT /api/v1/roles/{id}`
* **Permission Required**: `Permissions.Roles.Edit`

### Path Parameters
| Parameter | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `id` | `Guid` | Yes | The ID of the role to update. |

### Request Body
*Note: The `id` in the JSON body must match the `id` in the URL path.*
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Senior Warehouse Manager",
  "description": "Able to manage stock and edit couriers",
  "permissions": [
    "Permissions.Products.View",
    "Permissions.Products.Create",
    "Permissions.Couriers.View"
  ]
}
```

### Response
Returns a successful Standard Result with no value.

---

## 4. Activate Role

Activates a currently inactive Role. This operates purely locally inside the Order Management System DB.

* **Endpoint**: `PUT /api/v1/roles/{id}/activate`
* **Permission Required**: `Permissions.Roles.Activate`

### Path Parameters
| Parameter | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `id` | `Guid` | Yes | The ID of the role to activate. |

### Response
Returns a successful Standard Result with no value.

---

## 5. Deactivate Role

Deactivates a currently active Role. This operates purely locally inside the Order Management System DB (i.e., users with this role will no longer inherit its permissions locally).

* **Endpoint**: `PUT /api/v1/roles/{id}/deactivate`
* **Permission Required**: `Permissions.Roles.Deactivate`

### Path Parameters
| Parameter | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `id` | `Guid` | Yes | The ID of the role to deactivate. |

### Response
Returns a successful Standard Result with no value.

---

## 6. Delete Role

Deletes a role from the local database and subsequently from the Identity Provider.

* **Endpoint**: `DELETE /api/v1/roles/{id}`
* **Permission Required**: `Permissions.Roles.Delete`

### Path Parameters
| Parameter | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `id` | `Guid` | Yes | The ID of the role to delete. |

### Response
Returns a successful Standard Result with no value.

---

## 7. Get All Permissions (Tree Structure)

Retrieves a grouped list of all granular permissions available in the system. Use this to render the UI for creating/editing roles.

* **Endpoint**: `GET /api/v1/permissions`
* **Permission Required**: None (Just requires a valid authentication token)

### Response
Returns a successful Standard Result containing `List<PermissionGroupDto>`.

```json
{
  "isSuccess": true,
  "value": [
    {
      "groupName": "Couriers",
      "permissions": [
        { "name": "View", "value": "Permissions.Couriers.View" },
        { "name": "Create", "value": "Permissions.Couriers.Create" }
      ]
    },
    {
      "groupName": "Roles",
      "permissions": [
        { "name": "View", "value": "Permissions.Roles.View" }
      ]
    }
  ]
}
```

---

## 8. Get Current User Permissions

Retrieves a flattened list of all granular permissions the currently logged-in user possesses. Use this to determine which UI elements (buttons, menus) should be visible to the user.

* **Endpoint**: `GET /api/v1/users/me/permissions`
* **Permission Required**: None (Just requires a valid authentication token)

### Response
Returns a successful Standard Result containing a simple string array of the user's permissions.

```json
{
  "isSuccess": true,
  "value": [
    "Permissions.Products.View",
    "Permissions.Products.Create",
    "Permissions.Couriers.View"
  ]
}
```
