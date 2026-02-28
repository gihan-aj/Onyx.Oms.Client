# System Settings API Reference

This document outlines the REST API endpoints available for managing System Settings, particularly Tenant Profiles and Application Sequences.

## Base URL
All endpoints are relative to the API's base path: `/api/v1`

---

## 1. Tenant Profile
The Tenant Profile represents the global system configuration, including store details, regional settings, and UI preferences.

**Base Route:** `/settings/profile`

### 1.1 Get Tenant Profile
Retrieves the global tenant profile containing store settings. If no profile exists, a default one is automatically initialized based on `appsettings.json`.

* **Method:** `GET`
* **Route:** `/`
* **Authorization:** Required
* **Permission:** `Permissions.TenantSettings.View`

**Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "storeName": "Onyx Store",
  "legalName": "Onyx Corp",
  "taxRegistrationNumber": "TAX-12345",
  "contactEmail": "admin@onyx.local",
  "contactPhone": "+123456789",
  "storeAddress": {
    "street": "123 Main St",
    "city": "Metropolis",
    "state": "NY",
    "postalCode": "10001",
    "country": "USA"
  },
  "baseCurrency": "LKR",
  "weightUnit": "kg",
  "invoiceFooterText": "Thank you for your business!",
  "logoUrl": "https://example.com/logo.png",
  "preferencesJson": "{ \"theme\": \"dark\" }"
}
```

### 1.2 Update Store Info
Updates basic store information such as business name and contact details.

* **Method:** `PUT`
* **Route:** `/store-info`
* **Authorization:** Required
* **Permission:** `Permissions.TenantSettings.Edit`

**Request Body:**
```json
{
  "storeName": "Onyx Store",
  "legalName": "Onyx Corp",
  "taxRegistrationNumber": "TAX-12345",
  "contactEmail": "admin@onyx.local",
  "contactPhone": "+123456789"
}
```

**Responses:**
* `204 No Content`: Successful update.
* `400 Bad Request`: Validation failure (e.g., missing required fields, exceeding character lengths).

### 1.3 Update Regional Settings
Updates technical business settings such as weight units and currency.

* **Method:** `PUT`
* **Route:** `/regional-settings`
* **Authorization:** Required
* **Permission:** `Permissions.TenantSettings.Edit`

**Request Body:**
```json
{
  "baseCurrency": "LKR",
  "weightUnit": "kg"
}
```

**Responses:**
* `204 No Content`: Successful update.
* `400 Bad Request`: Validation failure (e.g., currency not exactly 3 characters).

### 1.4 Update Store Address
Updates the physical store address for the tenant profile.

* **Method:** `PUT`
* **Route:** `/address`
* **Authorization:** Required
* **Permission:** `Permissions.TenantSettings.Edit`

**Request Body:**
```json
{
  "storeAddress": {
    "street": "123 Main St",
    "city": "Metropolis",
    "state": "NY",
    "postalCode": "10001",
    "country": "USA"
  }
}
```

**Responses:**
* `204 No Content`: Successful update.
* `400 Bad Request`: Validation failure.

### 1.5 Update Preferences
Updates the JSON structured user interface preferences for the store.

* **Method:** `PUT`
* **Route:** `/preferences`
* **Authorization:** Required
* **Permission:** `Permissions.TenantSettings.Edit`

**Request Body:**
```json
{
  "preferencesJson": "{ \"theme\": \"dark\", \"sidebarCollapsed\": true }"
}
```

**Responses:**
* `204 No Content`: Successful update.
* `400 Bad Request`: Validation failure.

---

## 2. Application Sequences
Application Sequences handle the auto-generation of sequential strings used throughout the system (e.g., Order Numbers, SKUs).

**Base Route:** `/settings/sequences`

### 2.1 Get Sequence Value
Retrieves the current value for a given sequence ID.

* **Method:** `GET`
* **Route:** `/{id}`
* **Authorization:** Required
* **Permission:** `Permissions.AppSequences.View` (Assuming standard permission setup)

**Path Parameters:**
* `id` (string): The identifier of the sequence (e.g., `ORD`, `SKU`, `INV`).

**Response (200 OK):**
```json
1000
```
*(Returns an integer representing the current valid value)*

### 2.2 Update Sequence Value
Updates the current value for a given sequence ID. The new value cannot be less than the existing current value to prevent sequence collisions.

* **Method:** `PUT`
* **Route:** `/{id}`
* **Authorization:** Required
* **Permission:** `Permissions.AppSequences.Edit` (Assuming standard permission setup)

**Path Parameters:**
* `id` (string): The identifier of the sequence to update.

**Request Body:**
```json
1500
```
*(Sends a raw integer value)*

**Responses:**
* `204 No Content`: Successful update.
* `400 Bad Request`: Validation failure (e.g., new value is less than the current sequence value).
