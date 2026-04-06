# Form Page UI Structure Pattern

This document outlines the standard structural patterns and conventions to be used for Data Entry / Form views within the Onyx OMS Client application. This ensures users have a consistent contextual layout when creating or editing entities (e.g., Customers, Couriers).

## 1. General Grid Layout Structure

The `Page` should use a root `Grid` with a background set to `{ThemeResource ApplicationPageBackgroundThemeBrush}`.
The Grid should be divided into two primary rows:
1. **Toolbar / Header Navigation** (`Height="Auto"`)
2. **Scrollable Form Area** (`Height="*"`)

## 2. Toolbar and Header Navigation

Located in `Grid.Row="0"` with `Background="{ThemeResource LayerFillColorDefaultBrush}"` and sensible padding (`16,12`).

*   **Layout**: A 3-column `Grid`:
    *   **Left (Column 0)**: A "Back/Cancel" subtle round button.
    *   **Center (Column 1)**: A `StackPanel` containing the Form Title (e.g., "Edit Courier") and an inline Status Badge (if applicable, using `StatusBadgeStyle` depending on `IsActive` and `IsEditMode`).
    *   **Right (Column 2)**: Action Buttons, prominently the `Save Changes` accent button (typically bound to `SaveCommand`).

## 3. Scrollable Form Layout

Located in `Grid.Row="1"`. Enclosed in a `ScrollViewer` (vertical auto, horizontal disabled).

*   **Centering Magic**: 
    *   Use a 3-column Grid to horizontally clamp the maximum width of the form content without making it look detached on ultra-wide screens.
        *   Column 0: `Width="*"`
        *   Column 1: `Width="10000*"` `MaxWidth="1200"`
        *   Column 2: `Width="*"`
    *   The `StackPanel` holding the form resides in Column 1.
*   **Column Splitting**: 
    *   For forms with many fields, split the content area into two columns (e.g., `1*` and `1.2*`) using a `Grid` with `ColumnSpacing="16"`.
*   **Card Groups**:
    *   Group related fields into visual cards.
    *   Use a `StackPanel` with `Style="{StaticResource CardStackStyle}"`.
    *   Include a `TextBlock` title with `Style="{StaticResource BodyStrongTextBlockStyle}"`.
    *   Stack input controls vertically. 
    *   Combine smaller fields side-by-side using inner Grids with proportions.

## 4. Input Controls & Validation

*   **Read-Only Modes**: All fields must respect `<TextBox IsReadOnly="{x:Bind ViewModel.IsReadOnly, Mode=OneWay}" />`.
*   **Validation Errors**: Supply validation messages directly beneath fields using `TextBlock` with `SystemFillColorCriticalBrush` and `CaptionTextBlockStyle`.
*   **Saving State**: 
    *   Instead of disabling the whole UI, display an overarching `ProgressRing` or indeterminate `ProgressBar` when `IsBusy` / `IsLoading` is true.

## 5. ViewModel Capabilities

A standard Form View Model should inherit standard observable structures and provide:
*   `IsEditMode` property leveraging `SetProperty` (`INotifyPropertyChanged` is mandatory for badge logic).
*   `IsReadOnly` property syncing dependent properties.
*   `SaveCommand` and `CancelCommand`.
*   Error strings mapped to explicit fields (e.g. `NameError`).
*   `InitializeAsync(Dto)` orchestrating mapping logic and `IsLoading` orchestration.
