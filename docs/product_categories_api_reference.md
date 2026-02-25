# Product Categories API Reference

The Product Categories API provides endpoints for managing product categories within the Onyx.Oms system. It supports a hierarchical structure for organizing products with specific business rules governing depth, paths, and parent-child relationships.

**Base URL**: `/api/v1/product-categories`

---

## 1. Business Logic and Category Structure

Product Categories naturally form a tree hierarchy. To ensure the catalog remains organized and performant, strict rules are enforced.

### 1.1 Hierarchy Depth
The system enforces a **Maximum Depth of 2** (`MaxDepth = 2`):
*   **Level 0 (Root)**: Top-level category (e.g., "Men's Clothing").
*   **Level 1 (Sub-category)**: Direct child of a root category (e.g., "Shirts").
*   **Level 2 (Sub-sub-category)**: Child of a sub-category (e.g., "T-Shirts").

**Business Rule:** You cannot create or move a category to be deeper than Level 2.

### 1.2 Materialized Paths
To quickly retrieve category trees without complex database queries, the system stores materialized paths:
*   **`Path`**: A technical path containing the GUIDs of the category and its ancestors (e.g., `/RootId/ChildId/`).
*   **`NamePath`**: A human-readable breadcrumb path (e.g., `Men's Clothing / Shirts`).

### 1.3 Creation Rules
*   **Required Fields**: `name` must be provided.
*   **Uniqueness**: The category `name` must be unique **under the same parent**.
*   **Path Generation**: Upon creation, the system automatically generates the `Path` and `NamePath` based on the chosen parent.

### 1.4 Modification & Movement Rules
Moving a category (changing its `parentCategoryId`) triggers strict checks:
*   **Circular Reference Prevention**: A category cannot be its own parent.
*   **Deep Circular Prevention**: A category cannot be moved into one of its own sub-categories.
*   **Depth Re-evaluation**: Moving a category ensures that no category in its entire subtree will exceed the `MaxDepth` of 2.
*   **Path Cascade**: When moved, the system automatically updates the `Path` and `NamePath` recursively for the category and all its descendants.

### 1.5 Deactivation Rules
Categories use soft-deletes (`isActive` flag).
*   **Cascading Deactivation**: Deactivating a parent category will automatically deactivate all of its sub-categories recursively. Activating a category does not automatically activate its children.

---

## 2. Data Models

### 2.1 ProductCategoryDto (List View)
The standard flat representation of a Product Category.

| Property | Type | Description |
| :--- | :--- | :--- |
| `id` | `Guid` | Unique identifier for the category. |
| `name` | `string` | Name of the category. |
| `description` | `string` | (Optional) Description of the category. |
| `parentCategoryId` | `Guid` | (Optional) ID of the parent category. Null if root. |
| `parentCategoryName` | `string` | (Optional) Name of the parent category. |
| `level` | `int` | Depth level (0 for root, 1 for sub, 2 for sub-sub). |
| `path` | `string` | Hierarchical materialized ID path (e.g., `/RootId/ChildId/`). |
| `namePath` | `string` | Breadcrumb name path (e.g., `Root / Child / GrandChild`). |
| `iconUrl` | `string` | (Optional) Link to an icon image. |
| `color` | `string` | (Optional) Hex color code for UI styling. |
| `displayOrder` | `int` | Integer for controlling sorting in the UI. |
| `isActive` | `bool` | Indicates if the category is active. |

### 2.2 ProductCategoryTreeDto (Tree View)
The hierarchical representation used when fetching the category tree.

| Property | Type | Description |
| :--- | :--- | :--- |
| `id` | `Guid` | Unique identifier for the category. |
| `name` | `string` | Name of the category. |
| `description` | `string` | (Optional) Description of the category. |
| `level` | `int` | Depth level (0 for root, 1 for sub, 2 for sub-sub). |
| `iconUrl` | `string` | (Optional) Link to an icon image. |
| `color` | `string` | (Optional) Hex color code for UI styling. |
| `displayOrder` | `int` | Integer for controlling sorting in the UI. |
| `isActive` | `bool` | Indicates if the category is active. |
| `subCategories` | `List<ProductCategoryTreeDto>`| Nested sub-categories. |

---

## 3. Endpoints

### 3.1 Get All Categories (Flat List)
Retrieves a flat list of product categories, optionally filtered to only include leaf categories (those with no sub-categories), and by active status.

*   **URL**: `/api/v1/product-categories`
*   **Method**: `GET`
*   **Query Parameters**:
    *   `onlyLeaves` (boolean, optional): Set to `true` to only return categories without children. Default is `false`.
    *   `isActive` (boolean, optional): Filter by active status (`true`/`false`).

**Response (200 OK):**
Returns a list of `ProductCategoryDto`.
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "T-Shirts",
    "parentCategoryId": "c9281a8b-12d4-4a5c-a1d2-b8f9e2c7a0d4",
    "parentCategoryName": "Shirts",
    "level": 2,
    "path": "/1c28...d4/c928...d4/3fa8...a6/",
    "namePath": "Men's Clothing / Shirts / T-Shirts",
    "iconUrl": "https://example.com/icons/tshirt.png",
    "color": "#FF0000",
    "displayOrder": 1,
    "isActive": true
  }
]
```

### 3.2 Get Categories Paged
Retrieves a paginated list of product categories, with optional searching, filtering, and sorting.

*   **URL**: `/api/v1/product-categories/search`
*   **Method**: `GET`
*   **Query Parameters**:
    *   `page` (int, required): The page number to retrieve.
    *   `pageSize` (int, required): The number of items per page.
    *   `searchTerm` (string, optional): Search by name or description.
    *   `isActive` (boolean, optional): Filter by active status (`true`/`false`).
    *   `sortColumn` (string, optional): Column to sort by (e.g., `name`, `level`, `displayOrder`, `createdDate`). Default is level then display order.
    *   `sortOrder` (string, optional): Sort direction (`asc`/`desc`).
    *   `isValidParent` (boolean, optional): Set to `true` to only return categories that can have sub-categories (Level < 2).
    *   `isLeafOnly` (boolean, optional): Set to `true` to only return categories without children.

**Response (200 OK):**
Returns a `PagedResult` containing a list of `ProductCategoryDto`.
```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "T-Shirts",
      "description": "Short sleeve and long sleeve t-shirts",
      "parentCategoryId": "c9281a8b-12d4-4a5c-a1d2-b8f9e2c7a0d4",
      "parentCategoryName": "Shirts",
      "level": 2,
      "path": "/1c28...d4/c928...d4/3fa8...a6/",
      "namePath": "Men's Clothing / Shirts / T-Shirts",
      "iconUrl": "https://example.com/icons/tshirt.png",
      "color": "#FF0000",
      "displayOrder": 1,
      "isActive": true
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 50,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### 3.3 Get Category Tree
Retrieves the full hierarchy of product categories as a nested tree structure.

*   **URL**: `/api/v1/product-categories/tree`
*   **Method**: `GET`
*   **Query Parameters**:
    *   `isActive` (boolean, optional): Filter by active status (`true`/`false`). Deactivated ancestors hide themselves and all descendants.

**Response (200 OK):**
Returns a list of `ProductCategoryTreeDto` (Root categories, which contain nested sub-categories).

### 3.3 Create Category
Creates a new product category. Supports hierarchy.

*   **URL**: `/api/v1/product-categories`
*   **Method**: `POST`
*   **Request Body**:

| Property | Type | Required | Description |
| :--- | :--- | :--- | :--- |
| `name` | `string` | Yes | Name of the category. |
| `description` | `string` | No | Description of the category. |
| `parentCategoryId` | `Guid` | No | If provided, category is created under this parent. Max depth is 2. |
| `displayOrder` | `int` | Yes | Sorting order number. |
| `iconUrl` | `string` | No | Link to an icon image. |
| `color` | `string` | No | UI hex color code. |

**Example Request:**
```json
{
  "name": "Men's Clothing",
  "description": "Apparel for men",
  "parentCategoryId": null,
  "displayOrder": 0,
  "iconUrl": null,
  "color": "#000000"
}
```

**Response (200 OK):**
Returns the GUID of the created category.
```json
"c9281a8b-12d4-4a5c-a1d2-b8f9e2c7a0d4"
```

### 3.4 Update Category
Updates a product category. Handles moving categories (and their entire subtrees) to a new parent, updating names and recursively recalculating paths.

*   **URL**: `/api/v1/product-categories/{id}`
*   **Method**: `PUT`
*   **Path Parameters**:
    *   `id` (Guid): The ID of the category to update.
*   **Request Body**:
    *   Includes an `id` property which must match the URL path parameter. All other fields are the same as Create Category.

**Response (204 No Content):**
The update was successful.

### 3.5 Delete Category
Deletes a product category if it has no children. To delete heavily nested categories, you either delete children first or modify the parent ID.

*   **URL**: `/api/v1/product-categories/{id}`
*   **Method**: `DELETE`
*   **Path Parameters**:
    *   `id` (Guid): The ID of the product category.

**Response (204 No Content):**
The deletion was successful.

### 3.6 Activate Category
Activates a product category. Note: This does NOT recursively activate children.

*   **URL**: `/api/v1/product-categories/{id}/activate`
*   **Method**: `PUT`
*   **Path Parameters**:
    *   `id` (Guid): The ID of the product category.

**Response (204 No Content):**
Success.

### 3.7 Deactivate Category
Deactivates a product category. Note: This recursively deactivates all of its sub-categories.

*   **URL**: `/api/v1/product-categories/{id}/deactivate`
*   **Method**: `PUT`
*   **Path Parameters**:
    *   `id` (Guid): The ID of the product category.

**Response (204 No Content):**
Success.
