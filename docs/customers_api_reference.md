# Customers API Reference

This document outlines the API endpoints available for managing Customers in the Onyx.Oms system. You can use this reference to implement the UI features in the Desktop (WinUI) application.

All endpoints require authentication (Bearer token) and specific system permissions.

## Base URL
`api/v1/customers`

---

## 1. Get Customers (Paged)

Retrieves a paginated list of customers with optional searching (Name, Email, Phone) and sorting.

* **Endpoint**: `GET /api/v1/customers/search`
* **Permission Required**: `Permissions.Customers.View`

### Query Parameters
| Parameter | Type | Required | Default | Description |
| :--- | :--- | :--- | :--- | :--- |
| `Page` | `int` | No | `1` | The page number to retrieve. |
| `PageSize` | `int` | No | `10` | Number of items per page. |
| `SearchTerm` | `string` | No | `null` | Wildcard search term (searches by Name, Email, Phone). |
| `SortColumn` | `string` | No | `null` | The property to sort by. |
| `SortOrder` | `string` | No | `null` | The sort direction (`"asc"` or `"desc"`). |
| `IsActive` | `bool` | No | `null` | Filter by active status (`true` for active, `false` for inactive customers). If omitted, returns all. |

### Response Details
The response will be a wrapped standard Result object containing a `PagedResult<CustomerDto>`.

**Response Body (`Value` property contents):**
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Acme Corp",
      "email": "contact@acmecorp.com",
      "primaryPhone": "+1-555-0123",
      "secondaryPhone": null,
      "address": {
        "street": "123 Business Rd",
        "city": "Metropolis",
        "state": "NY",
        "postalCode": "10001",
        "country": "USA"
      },
      "notes": "Premium customer",
      "isActive": true,
      "createdDate": "2023-10-01T12:00:00+00:00"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 50,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

## 2. Get Customer by ID

Retrieves a customer's details by their unique identifier.

* **Endpoint**: `GET /api/v1/customers/{id}`
* **Permission Required**: `Permissions.Customers.View`

### Path Parameters
| Parameter | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `id` | `Guid` | Yes | The ID of the customer to retrieve. |

### Response Details
Returns a wrapped standard Result containing the `CustomerDto`.

```json
{
  "isSuccess": true,
  "value": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Acme Corp",
    "email": "contact@acmecorp.com",
    "primaryPhone": "+1-555-0123",
    "secondaryPhone": null,
    "address": {
      "street": "123 Business Rd",
      "city": "Metropolis",
      "state": "NY",
      "postalCode": "10001",
      "country": "USA"
    },
    "notes": "Premium customer",
    "isActive": true,
    "createdDate": "2023-10-01T12:00:00+00:00"
  }
}
```

---

## 3. Create Customer

Creates a new customer record with address.

* **Endpoint**: `POST /api/v1/customers`
* **Permission Required**: `Permissions.Customers.Create`

### Request Body
```json
{
  "name": "Acme Corp",
  "email": "contact@acmecorp.com",
  "primaryPhone": "+1-555-0123",
  "secondaryPhone": null,
  "street": "123 Business Rd",
  "city": "Metropolis",
  "state": "NY",
  "postalCode": "10001",
  "country": "USA",
  "notes": "Premium customer"
}
```

### Response
Returns a Standard Result containing the new Customer's `Guid` ID.
```json
{
  "isSuccess": true,
  "value": "11b712fa-3e6f-4d33-91db-de5afdf8f5bc"
}
```

---

## 4. Update Customer

Updates an existing customer's details.

* **Endpoint**: `PUT /api/v1/customers/{id}`
* **Permission Required**: `Permissions.Customers.Edit`

### Path Parameters
| Parameter | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `id` | `Guid` | Yes | The ID of the customer to update. |

### Request Body
*Note: The `id` in the JSON body must match the `id` in the URL path.*
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Acme Corp Worldwide",
  "email": "global@acmecorp.com",
  "primaryPhone": "+1-555-0999",
  "secondaryPhone": null,
  "street": "456 Global Ave",
  "city": "Metropolis",
  "state": "NY",
  "postalCode": "10002",
  "country": "USA",
  "notes": "Updated to global contact"
}
```

### Response
Returns a successful Standard Result with no value.

---

## 5. Activate Customer

Activates a previously deactivated customer.

* **Endpoint**: `PUT /api/v1/customers/{id}/activate`
* **Permission Required**: `Permissions.Customers.Activate`

### Path Parameters
| Parameter | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `id` | `Guid` | Yes | The ID of the customer to activate. |

### Response
Returns a successful Standard Result with no value.

---

## 6. Deactivate Customer

Deactivates a customer.

* **Endpoint**: `PUT /api/v1/customers/{id}/deactivate`
* **Permission Required**: `Permissions.Customers.Deactivate`

### Path Parameters
| Parameter | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `id` | `Guid` | Yes | The ID of the customer to deactivate. |

### Response
Returns a successful Standard Result with no value.

---

## 7. Delete Customer

Deletes a customer by their unique identifier.

* **Endpoint**: `DELETE /api/v1/customers/{id}`
* **Permission Required**: `Permissions.Customers.Delete`

### Path Parameters
| Parameter | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `id` | `Guid` | Yes | The ID of the customer to delete. |

### Response
Returns a successful Standard Result with no value.
