# Products API Reference

This document outlines the API endpoints for managing Products within Onyx.Oms.

## 1. Create Product

Creates a new product aggregate, optionally including its associated product variants and product images in a single atomic request.

*   **URL:** `/api/v1/products`
*   **Method:** `POST`
*   **Tags:** `Products`
*   **Permissions Required:** `Permissions.Products.Create`

### 1.1 Request Payload

The request body should be a JSON object matching the `CreateProductCommand`.

```json
{
  "name": "string (Max 200) - Required",
  "baseSku": "string (Max 100) - Optional, will be auto-generated if omitted",
  "description": "string (Optional)",
  "categoryId": "00000000-0000-0000-0000-000000000000 (Required)",
  "brand": "string (Optional)",
  "material": "string (Optional)",
  "gender": "integer/string (Enum: Unknown=0, Male=1, Female=2, Unisex=3)",
  "baseCostAmount": "decimal (>= 0)",
  "baseCostCurrency": "string (Max 3, e.g., 'LKR') - Required & must match Tenant Profile",
  "basePriceAmount": "decimal (>= 0)",
  "basePriceCurrency": "string (Max 3, e.g., 'LKR') - Required & must match Tenant Profile",
  "baseWeightValue": "decimal (>= 0)",
  "baseWeightUnit": "string (Max 10, e.g., 'kg') - Required & must match Tenant Profile",
  "hasColor": "boolean",
  "hasSize": "boolean",
  "tags": [ "string" ],
  "variants": [
    {
      "sku": "string (Max 100) - Optional, will be auto-generated if omitted",
      "color": "string (Max 50) - Required if product HasColor is true",
      "size": "string (Max 50) - Required if product HasSize is true",
      "costAmount": "decimal (>= 0) - Optional fallback to BaseCostAmount",
      "priceAmount": "decimal (>= 0) - Optional fallback to BasePriceAmount",
      "weightValue": "decimal (>= 0) - Optional fallback to BaseWeightValue",
      "stockOnHand": "integer (>= 0)"
    }
  ],
  "images": [
    {
      "url": "string (Max 2048) - Required",
      "displayOrder": "integer",
      "isMain": "boolean",
      "color": "string (Max 50) - Optional. Must match a variant color if provided."
    }
  ]
}
```

### 1.2 Validations & Business Rules

#### Structural Validation (FluentValidation)
*   **Lengths:** `Name` (200), `BaseSku` (100), `BaseCostCurrency` / `BasePriceCurrency` (3), `BaseWeightUnit` (10), `Variant Sku` (100).
*   **Positivity:** All `Amount`, `Value`, and `StockOnHand` properties must be `>= 0`.

#### Domain Validations (Command Handler)
*   **Tenant Mapping:** The requested currencies (`baseCostCurrency`, `basePriceCurrency`) and weight unit (`baseWeightUnit`) **must exactly match** the single global `TenantProfile` settings.
*   **Base SKU Generation:** If `BaseSku` is null or whitespace, the system generates a sequential SKU locally using `AppSequenceService` (e.g. `PROD-0004`). If provided, it must be globally unique.
*   **Category Validation:** `categoryId` must exist in the database.
*   **Variant SKU Generation:** If a variant's `Sku` is omitted, the domain `SkuGenerator` service will auto-generate it using the Base SKU + Color Code + Size Code (e.g. `PROD-0004-RED-XL`).
*   **Variant SKU Uniqueness:** All sent Variant SKUs (provided or generated) must be unique within the request payload and globally against the database.
*   **Attribute Flags:** 
    *   If `hasColor` is true, all variant DTOs must provide a non-empty `Color`.
    *   If `hasSize` is true, all variant DTOs must provide a non-empty `Size`.
*   **Image Color Tagging:** If an Image provides a `Color`:
    *   The Product's `hasColor` must be true.
    *   The specified color must exactly match one of the `Color`s provided in the `variants` array.

### 1.3 Responses
*   `200 OK`: Returns the raw `Guid` of the newly created `Product`.
*   `400 Bad Request`: Validation failure (FluentValidation or Domain rules). 
*   `404 Not Found`: Category not found.
*   `409 Conflict`: Duplicate Base or Variant SKUs.

---

## 2. Get Products (Paged)

Retrieves a paginated list of base products, aggregating data from related entities suitable for a catalog data grid.

*   **URL:** `/api/v1/products/search`
*   **Method:** `GET`
*   **Tags:** `Products`
*   **Permissions Required:** `Permissions.Products.View`

### 2.1 Query Parameters

This endpoint accepts form query parameters bound to `GetProductsPagedQuery`.

| Parameter | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `Page` | Integer | `1` | The page number to retrieve. |
| `PageSize` | Integer | `10` | The number of items per page. |
| `SearchTerm` | String | null | Case-insensitive search on Product `Name`, `BaseSku`, and `Brand`. |
| `SortColumn` | String | null | Column to sort by (`Name`, `BaseSku`, `Category`, `Brand`, `IsActive`, `CreatedDate`). |
| `SortOrder` | String | null | Sort direction (`asc` or `desc`). Default is descending. |
| `IsActive` | Boolean | null | Filter by active/inactive products. |
| `CategoryId` | Guid | null | Filter by a specific Category ID. |
| `HasColor` | Boolean | null | Filter products that have color variants enabled. |
| `HasSize` | Boolean | null | Filter products that have size variants enabled. |

### 2.2 Response Body

Returns a `PagedResult<ProductDto>`.

```json
{
  "items": [
    {
      "id": "e7bfa51a-d0ba-4ffb-8217-08dcd37c95a0",
      "name": "Classic T-Shirt",
      "baseSku": "PROD-0004",
      "categoryName": "T-Shirts",
      "brand": "Onyx Basic",
      "isActive": true,
      "hasColor": true,
      "hasSize": true,
      "basePriceAmount": 2500.00,
      "basePriceCurrency": "LKR",
      "baseCostAmount": 1200.00,
      "baseCostCurrency": "LKR",
      "totalStock": 150, 
      "createdOnUtc": "2026-02-28T08:00:00.0000000+00:00"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 45,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

*Note: `totalStock` is a dynamically summed aggregation of `StockOnHand` from all associated `ProductVariant` entities for that specific base product.*
