# Product Category Management

This document explains how Product Categories are structured and created within Onyx.Oms.

## 1. Category Structure

Product Categories in Onyx.Oms use a hierarchical structure with a strict maximum depth to ensure the catalog remains organized and performant.

### 1.1 Hierarchy Depth
The system enforces a **Maximum Depth of 3** (`MaxDepth = 3`):
*   **Level 0 (Root)**: Top-level category (e.g., "Men's Clothing").
*   **Level 1 (Sub-category)**: Direct child of a root category (e.g., "Shirts").
*   **Level 2 (Sub-sub-category)**: Child of a sub-category (e.g., "T-Shirts").
*   **Level 3 (Sub-sub-sub-category)**: Child of a sub-sub-category (e.g., "T-Shirts").

*Business Rule:* You cannot create a category deeper than Level 3.

### 1.2 Materialized Paths
To quickly retrieve category trees without complex database queries, the system uses two types of paths:
*   **`Path`**: A technical path containing the GUIDs of the category and its ancestors (e.g., `/RootId/ChildId/`). This allows quickly finding all descendants of a category.
*   **`NamePath`**: A human-readable breadcrumb path (e.g., `Men's Clothing / Shirts / T-Shirts`).

## 2. Creating a Category

When creating a category, the following rules and processes apply:

### 2.1 Required Fields
*   **Name**: Must be provided and cannot be empty.
*   **Parent Category ID** (Optional): If provided, the new category will be created under this parent. If omitted, it becomes a Root category.

### 2.2 Optional Metadata
*   **Description**: Additional details about the category.
*   **Display Order**: An integer to control sorting in the UI (e.g., 0, 1, 2).
*   **IconUrl**: A link to an icon image for the category.
*   **Color**: A hex color code for UI styling.
*   **Specifications**: A list of dynamic specification definitions (e.g., "Size", "Color") that products in this category will require. Max of 30 specifications per category.

### 2.3 Business Rules for Creation
1.  **Uniqueness**: The category `Name` must be unique **under the same parent**. You cannot have two "Shirts" categories under "Men's Clothing", but you can have one under "Men's" and another under "Women's".
2.  **Depth Validation**: The system checks if assigning the requested parent will exceed the `MaxDepth` of 2. If the parent is already at Level 2, the creation will be rejected.
3.  **Path Generation**: Upon creation, the system automatically generates the `Path` and `NamePath` based on the chosen parent.
4.  **Specification Validation**: If specifications are provided, they must not have duplicate `Key` values, and the total count cannot exceed 30.

## 3. Modifying a Category

### 3.1 Updating Details
You can update the Name, Description, Display Order, Icon, and Color. Changing the name will attempt to automatically update its own `NamePath` suffix.

### 3.2 Updating Specifications
You can update the dynamic specifications for a category. The system will validate that the new list contains no duplicate `Key` values and does not exceed the limit of 30 specifications.

### 3.3 Changing Parents (Moving Categories)
Moving a category is supported but involves strict checks:
*   **Circular Reference Prevention**: A category cannot be its own parent.
*   **Deep Circular Prevention**: A category cannot be moved *into* one of its own sub-categories.
*   **Depth Re-evaluation**: The system ensures that moving the category (and its children) will not cause any category in the subtree to exceed the `MaxDepth` of 2.
*   **Path Cascade**: When moved, the system automatically recalculates the `Path` and `NamePath` for the category and recursively updates all its sub-categories.

## 4. Deactivation
Categories use soft-deletes (`IsActive` flag).
*   **Cascading Deactivation**: Deactivating a parent category will automatically deactivate all of its sub-categories.
