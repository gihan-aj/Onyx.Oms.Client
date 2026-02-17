# Couriers API Reference

The Couriers API provides endpoints for managing courier entities within the Onyx.Oms system.

**Base URL**: `/api/v1/couriers`

## Data Models

### Courier Object
The standard representation of a Courier.

| Property | Type | Description |
| :--- | :--- | :--- |
| `id` | `Guid` | Unique identifier for the courier. |
| `name` | `string` | Name of the courier service. |
| `contactPerson` | `string` | (Optional) Primary contact person. |
| `primaryPhone` | `string` | (Optional) Primary phone number. |
| `secondaryPhone` | `string` | (Optional) Secondary phone number. |
| `websiteUrl` | `string` | (Optional) Website URL. |
| `isActive` | `bool` | Indicates if the courier is active. |

---

## Endpoints

### 1. Get All Couriers
Retrieves a list of all couriers, optionally filtering by active status.

- **URL**: `/api/v1/couriers`
- **Method**: `GET`
- **Query Parameters**:
    - `isActive` (boolean, optional): Filter by active status (true/false).

**Response (200 OK):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "DHL Express",
    "contactPerson": "John Doe",
    "primaryPhone": "+1234567890",
    "secondaryPhone": null,
    "websiteUrl": "https://dhl.com",
    "isActive": true
  }
]
```

### 2. Get Courier by ID
Retrieves details of a specific courier.

- **URL**: `/api/v1/couriers/{id}`
- **Method**: `GET`
- **Path Parameters**:
    - `id` (Guid): The ID of the courier.

**Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "DHL Express",
  "contactPerson": "John Doe",
  "primaryPhone": "+1234567890",
  "secondaryPhone": null,
  "websiteUrl": "https://dhl.com",
  "isActive": true
}
```

**Response (404 Not Found):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "extensions": {
    "errors": [
      {
        "code": "Courier.NotFound",
        "description": "Courier not found."
      }
    ]
  }
}
```

### 3. Create Courier
Creates a new courier record.

- **URL**: `/api/v1/couriers`
- **Method**: `POST`
- **Request Body**:

| Property | Type | Required | Max Length | Description |
| :--- | :--- | :--- | :--- | :--- |
| `name` | `string` | Yes | 200 | Unique name of the courier. |
| `contactPerson` | `string` | No | 200 | Name of contact person. |
| `primaryPhone` | `string` | No | 50 | Primary phone number. |
| `secondaryPhone` | `string` | No | 50 | Secondary phone number. |
| `websiteUrl` | `string` | No | 500 | Courier's website URL. |
| `trackingUrlTemplate` | `string` | No | 500 | Template for tracking URLs. |

**Example Request:**
```json
{
  "name": "FedEx",
  "contactPerson": "Jane Smith",
  "primaryPhone": "9876543210"
}
```

**Response (200 OK):**
Returns the GUID of the created courier.
```json
"3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

**Response (400 Bad Request):**
Validation errors.

### 4. Update Courier
Updates an existing courier.

- **URL**: `/api/v1/couriers/{id}`
- **Method**: `PUT`
- **Path Parameters**:
    - `id` (Guid): The ID of the courier to update.
- **Request Body**:
    - Same as Create Courier, but `id` in body must match URL.

**Example Request:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "FedEx Updated",
  "contactPerson": "Jane Smith",
  "primaryPhone": "9876543210"
}
```

**Response (204 No Content):**
The update was successful.

### 5. Delete Courier
Deletes a courier.

- **URL**: `/api/v1/couriers/{id}`
- **Method**: `DELETE`
- **Path Parameters**:
    - `id` (Guid): The ID of the courier.

**Response (204 No Content):**
The deletion was successful.

### 6. Activate Courier
Activates a courier account.

- **URL**: `/api/v1/couriers/{id}/activate`
- **Method**: `PUT`
- **Path Parameters**:
    - `id` (Guid): The ID of the courier.

**Response (204 No Content):**
Success.

### 7. Deactivate Courier
Deactivates a courier account.

- **URL**: `/api/v1/couriers/{id}/deactivate`
- **Method**: `PUT`
- **Path Parameters**:
    - `id` (Guid): The ID of the courier.

**Response (204 No Content):**
Success.

### 8. Search Couriers (Paged)
Retrieves a paginated list of couriers with searching and sorting.

- **URL**: `/api/v1/couriers/search`
- **Method**: `GET`
- **Query Parameters**:

| Parameter | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `Page` | `int` | 1 | The page number to retrieve. |
| `PageSize` | `int` | 10 | Number of items per page. |
| `SearchTerm` | `string` | null | Search text to filter couriers. |
| `SortColumn` | `string` | null | Column to sort by (e.g., "Name"). |
| `SortOrder` | `string` | "asc" | Sort direction ("asc" or "desc"). |
| `IsActive` | `bool` | null | Filter by active status. |

**Example Request:**
`GET /api/v1/couriers/search?Page=1&PageSize=10&SearchTerm=DHL&SortColumn=Name&SortOrder=asc`

**Response (200 OK):**
Returns a `PagedResult` object.
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "DHL Express",
      "contactPerson": "John Doe",
      "isActive": true
      // ... other properties
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```
