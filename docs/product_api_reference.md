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
  "baseCost": {
    "amount": "decimal (>= 0)",
    "currency": "string (Max 3, e.g., 'LKR') - Default 'LKR'"
  },
  "basePrice": {
    "amount": "decimal (>= 0)",
    "currency": "string (Max 3, e.g., 'LKR') - Default 'LKR'"
  },
  "baseWeight": {
    "value": "decimal (>= 0)",
    "unit": "string (Max 10, e.g., 'kg') - Default 'kg'"
  },
  "baseStockOnHand": "integer (>= 0) - Optional, used only if the product has NO variants",
  "options": [
    {
      "name": "string (e.g., 'Color') - Required",
      "values": [ "string (e.g., 'Red', 'Blue')" ]
    }
  ],
  "specifications": {
    "string (key)": "string (value)"
  },
  "variants": [
    {
      "sku": "string (Max 100) - Optional, will be auto-generated if omitted",
      "attributes": [
        {
          "name": "string (e.g., 'Color')",
          "value": "string (e.g., 'Red')"
        }
      ],
      "cost": {
        "amount": "decimal (>= 0)",
        "currency": "string (Max 3, e.g., 'LKR')"
      },
      "price": {
        "amount": "decimal (>= 0)",
        "currency": "string (Max 3, e.g., 'LKR')"
      },
      "weight": {
        "value": "decimal (>= 0)",
        "unit": "string (Max 10, e.g., 'kg')"
      },
      "stockOnHand": "integer (>= 0)"
    }
  ],
  "images": [
    {
      "url": "string (Max 2048) - Required",
      "displayOrder": "integer",
      "isMain": "boolean",
      "optionName": "string (Max 50) - Optional (e.g., 'Color')",
      "optionValue": "string (Max 50) - Optional (e.g., 'Red')"
    }
  ],
  "tags": [ "string" ]
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
*   **Dynamic Matrix Match:** Any provided variant must have exactly the same number of attributes as the product's options, and its attribute values must exist in the product options allowed values list.
*   **Image Tagging:** If an Image provides an `optionName` and `optionValue`:
    *   The Product must have that option name defined in its `options`.
    *   The `optionValue` must exactly match one of the allowed values for that option.

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
| `SearchTerm` | String | null | Case-insensitive search on Product `Name`, `BaseSku`, `Description` and `Tags`. |
| `SortColumn` | String | null | Column to sort by (`Name`, `BaseSku`, `CategoryName`, `BasePrice`, `IsActive`, `CreatedDate`). |
| `SortOrder` | String | null | Sort direction (`asc` or `desc`). Default is descending. |
| `IsActive` | Boolean | null | Filter by active/inactive products. |
| `CategoryId` | Guid | null | Filter by a specific Category ID. |
| `HasVariants` | Boolean | null | Filter products that have variants enabled. |

### 2.2 Response Body

Returns a `PagedResult<ProductDto>`.

```json
{
  "items": [
    {
      "id": "e7bfa51a-d0ba-4ffb-8217-08dcd37c95a0",
      "name": "Classic T-Shirt",
      "baseSku": "PROD-0004",
      "categoryId": "20b2fbe8-3c35-46ee-bc00-dc21d51a6575",
      "categoryName": "T-Shirts",
      "basePriceAmount": 2500.00,
      "basePriceCurrency": "LKR",
      "mainImageUrl": "https://example.com/image.jpg",
      "hasVariants": true,
      "isActive": true,
      "createdOnUtc": "2026-02-28T08:00:00.0000000+00:00",
      "lastModifiedOnUtc": null
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 45,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

## 3. Get Product by ID

Retrieves the detailed information of a specific product by its ID, including its specifications, variants, options, and images.

*   **URL:** `/api/v1/products/{id}`
*   **Method:** `GET`
*   **Tags:** `Products`
*   **Permissions Required:** `Permissions.Products.View`

### 3.1 Path Parameters

| Parameter | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `id` | Guid | Yes | The unique identifier of the product. |

### 3.2 Response Body

Returns a `ProductDetailsDto` containing the full product aggregate. Note that for products without variants, `variants` will be empty, and the default variant's inventory will map directly to `stockOnHand` and `reservedQuantity` on the root object.

```json
{
  "id": "e7bfa51a-d0ba-4ffb-8217-08dcd37c95a0",
  "name": "Classic T-Shirt",
  "baseSku": "PROD-0004",
  "description": "A comfortable classic t-shirt.",
  "categoryId": "20b2fbe8-3c35-46ee-bc00-dc21d51a6575",
  "categoryName": "T-Shirts",
  "categoryPath": "/20b2fbe8-3c35-46ee-bc00-dc21d51a6575/",
  "specifications": [
    {
      "key": "Material",
      "label": "Material",
      "value": "Cotton"
    },
    {
      "key": "Fit",
      "label": "Fit",
      "value": "Regular"
    }
  ],
  "baseCostAmount": 1500.00,
  "baseCostCurrency": "LKR",
  "basePriceAmount": 2500.00,
  "basePriceCurrency": "LKR",
  "baseWeightAmount": 0.2,
  "baseWeightCurrency": "kg",
  "hasVariants": true,
  "stockOnHand": 150,
  "reservedQuantity": 10,
  "options": [
    {
      "name": "Color",
      "dispalyOrder": 1,
      "values": [
        "Red",
        "Blue"
      ]
    }
  ],
  "variants": [
    {
      "id": "12345678-1234-1234-1234-123456789012",
      "sku": "PROD-0004-RED",
      "attributes": [
        {
          "name": "Color",
          "value": "Red"
        }
      ],
      "costAmount": 1500.00,
      "costCurrency": "LKR",
      "priceAmount": 2500.00,
      "priceCurrency": "LKR",
      "weightAmount": 0.2,
      "weightCurrency": "kg",
      "stockOnHand": 80,
      "reservedQuantity": 5,
      "isActive": true
    }
  ],
  "images": [
    {
      "id": "87654321-4321-4321-4321-210987654321",
      "url": "https://example.com/image.jpg",
      "displayOrder": 1,
      "isMain": true,
      "optionName": "Color",
      "optionValue": "Red"
    }
  ],
  "isActive": true
}
```

### 3.3 Status Codes

*   `200 OK`: Product found and returned.
*   `404 Not Found`: Product with the specified `id` does not exist.

---

## 4. Edit Product (Atomic Slices)

The product edit functionality follows a Vertical Slice Architecture, providing atomic endpoints for different sections of the product data. This ensures safer concurrent edits and better performance.

### 4.1 Update Basic Info
Updates top-level details of a product.

*   **URL:** `/api/v1/products/{id}/basic-info`
*   **Method:** `PUT`
*   **Payload (`UpdateProductBasicInfoCommand`):**
    ```json
    {
      "id": "guid",
      "name": "string",
      "description": "string",
      "baseSku": "string",
      "categoryId": "guid",
      "tags": [ "string" ]
    }
    ```

### 4.2 Update Base Logistics
Updates the fallback pricing and weight for the product matrix.

*   **URL:** `/api/v1/products/{id}/base-logistics`
*   **Method:** `PUT`
*   **Payload (`UpdateProductBaseLogisticsCommand`):**
    ```json
    {
      "id": "guid",
      "baseCost": { "amount": 0, "currency": "LKR" },
      "basePrice": { "amount": 0, "currency": "LKR" },
      "baseWeight": { "value": 0, "unit": "kg" }
    }
    ```

### 4.3 Update Specifications
Updates the dynamic category-based specifications.

*   **URL:** `/api/v1/products/{id}/specifications`
*   **Method:** `PUT`
*   **Payload (`UpdateProductSpecificationsCommand`):**
    ```json
    {
      "id": "guid",
      "specifications": { "Color": "Red", "Material": "Cotton" }
    }
    ```

### 4.4 Update Options Matrix
Updates the root options (Axes) for the product variant matrix.
**Note on Safe Deletion:** Removing an option value (e.g., "Red") from this array will automatically trigger a soft-delete for any existing `ProductVariant` that relies on that value.

*   **URL:** `/api/v1/products/{id}/options`
*   **Method:** `PUT`
*   **Payload (`UpdateProductOptionsCommand`):**
    ```json
    {
      "id": "guid",
      "options": [
        { "name": "Color", "values": ["Red", "Blue"] }
      ]
    }
    ```

### 4.5 Toggle Product Variants (HasVariants)
Transitions a product between Variant Matrix mode and Variant-less mode.

*   **URL:** `/api/v1/products/{id}/toggle-variants`
*   **Method:** `PUT`
*   **Payload (`ToggleProductVariantsCommand`):**
    ```json
    {
      "id": "guid",
      "hasVariants": true
    }
    ```
**Behavior:**
*   **`true` -> `false`:** The system will soft-delete all existing variants in the matrix, clear the `options` array, and automatically create a single "Default Variant" (with empty attributes) to track logistics.
*   **`false` -> `true`:** The system will soft-delete the existing "Default Variant". The client must subsequently call the `Update Options Matrix` endpoint to define the new axes.

### 4.6 Default Variant Logistics (Variant-less Products)
Updates inventory and pricing for a product where `hasVariants` is `false`.

*   **URL:** `/api/v1/products/{id}/default-variant-logistics`
*   **Method:** `PUT`
*   **Payload (`UpdateDefaultVariantLogisticsCommand`):**
    ```json
    {
      "productId": "guid",
      "sku": "string",
      "cost": { "amount": 0, "currency": "LKR" },
      "price": { "amount": 0, "currency": "LKR" },
      "weight": { "value": 0, "unit": "kg" },
      "stockOnHand": 0
    }
    ```

### 4.7 Update Variant Logistics (Matrix Products)
Updates logistics for a specific variant within the matrix.

*   **URL:** `/api/v1/products/{productId}/variants/{variantId}/logistics`
*   **Method:** `PUT`
*   **Payload (`UpdateProductVariantLogisticsCommand`):**
    ```json
    {
      "productId": "guid",
      "variantId": "guid",
      "cost": { "amount": 0, "currency": "LKR" },
      "price": { "amount": 0, "currency": "LKR" },
      "weight": { "value": 0, "unit": "kg" },
      "stockOnHand": 0
    }
    ```

### 4.8 Add Product Variant
Adds a new variant to an existing matrix product. Properties must match the defined options.

*   **URL:** `/api/v1/products/{productId}/variants`
*   **Method:** `POST`
*   **Payload (`AddProductVariantCommand`):**
    ```json
    {
      "productId": "guid",
      "sku": "string",
      "attributes": [ { "name": "Color", "value": "Green" } ],
      "cost": { "amount": 0, "currency": "LKR" },
      "price": { "amount": 0, "currency": "LKR" },
      "weight": { "value": 0, "unit": "kg" },
      "stockOnHand": 0
    }
    ```

### 4.9 Delete Product Variant
Soft-deletes a specific variant from the matrix.

*   **URL:** `/api/v1/products/{productId}/variants/{variantId}`
*   **Method:** `DELETE`
