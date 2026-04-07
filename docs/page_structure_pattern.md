# List Page UI Structure Pattern

This document outlines the standard structural patterns and conventions to be used for List/Grid views within the Onyx OMS Client application. Following this pattern ensures a consistent and robust user experience across the desktop client.

## 1. General Grid Layout Structure

The `Page` should use a root `Grid` divided into three primary rows:
1. **Header & Filters** (`Height="Auto"`)
2. **DataGrid & Empty/Loading States** (`Height="*"`)
3. **Pagination & Footer** (`Height="Auto"`)

## 2. Header and Toolbar

The top container provides user search, primary actions, and local filters.

*   **Layout**: Use a `Grid` with `Background="{ThemeResource LayerFillColorDefaultBrush}"` and sensible padding (`16,12`).
*   **Top Row**: 
    *   **Title** (Left)
    *   **Search Box**: `AutoSuggestBox` mapped to `SearchTerm` and `SearchCommand`.
    *   **Global Actions**: `Refresh` (icon) and `New [Entity]` button (Accent colored) grouped in a `StackPanel`.
*   **Bottom Row (Filters)**: 
    *   Local state filters, predominantly a `Status` filter (`ComboBox` bound to `SelectedStatus`).
    *   A "Clear Filters" subtle button.

## 3. DataGrid and Action Menus

The main content area displays tabular data via standard `DataGrid`.

*   **Grid Definition**: 
    *   `IsReadOnly="True"`, `SelectionMode="Single"`.
    *   Bind `ItemsSource` securely.
    *   Configure `Sorting` event attached to ViewModel properties.
    *   **Horizontal Scroll Constraints (Critical):** WinUI DataGrid column layouts can overflow and distort without explicit width bounds. 
        Wrap the `DataGrid` in a `ScrollViewer` granting horizontal scroll capability. 
        Bind the `DataGrid`'s width upward: `Width="{Binding ActualWidth, ElementName=TableContainer}"`, disable native horizontal scrolling `HorizontalScrollBarVisibility="Hidden"`, and hardcode the table's absolute sum `MinWidth` (e.g., `880`).
    *   **Row-Level Context Menu:** Map standard actions to `ContextFlyout` onto `<toolkit:DataGrid.RowStyle>` triggering contextual capabilities upon right-click.
*   **Actions Column Pattern (Critical)**:
    *   To prevent row selection conflicts and improve touch/mouse hit-testing, employ a single "More" button containing a `MenuFlyout` for row-level actions (View, Edit, Activate/Deactivate, Delete).
    *   **XAML Signature**:
        ```xml
        <Button Style="{StaticResource SubtleButtonStyle}" 
                Background="Transparent"
                HorizontalAlignment="Stretch" VerticalAlignment="Stretch"
                ToolTipService.ToolTip="Actions"
                Tapped="OnActionButtonTapped">
            <FontIcon FontFamily="Segoe MDL2 Assets" Glyph="&#xE712;" />
            <Button.Flyout>
                <MenuFlyout> ... </MenuFlyout>
            </Button.Flyout>
        </Button>
        ```
    *   **Code-Behind Mapping**: The `Tapped` event must set `e.Handled = true;` to avoid DataGridRow selection hijacking, and then manually invoke `.ShowAt(btn)` on the flyout.

## 4. Interstitial States

*   **Loading**: A `ProgressRing` overlay tied to an `IsListLoading` view-model boolean. Use a dimmed translucent background to preserve context.
*   **Empty States**: A centered instructional `StackPanel` presenting a clear icon, status message, and a clear-search button, governed by a `HasNoData` view-model boolean.

## 5. ViewModel Capabilities

A standard List View Model should inherit standard observable structures and provide:
*   `ObservableCollection<Dto>` for current items.
*   `SelectedStatus` property intercepting updates to trigger page resets.
*   `SearchTerm` and explicit `ClearFiltersCommand`.
*   Standardized `Page`, `PageSize`, and `TotalCount` properties governing `LoadDataAsync()` queries.
