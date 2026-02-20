# Feature Development Guide

This guide outlines the standard practices and patterns used in the **Onyx.Oms.Client.Desktop** application. Follow these steps when creating new features to ensure consistency and maintainability.

## 1. Architecture Overview

We use a **Vertical Slice Architecture** organized by **Feature**.
-   **Location**: `src/Onyx.Oms.Client.Desktop/Features/[FeatureName]` (e.g., `Features/Couriers`)
-   **Contents**: All related files (Page, ViewModel, API, DTOs) should live in this folder.

## 2. Step-by-Step Implementation

### Step 1: Define API & DTOs
Define the data structures and API endpoints first. We use **Refit** for API clients.

**File**: `Features/[Feature]/I[Feature]Api.cs`
```csharp
using Onyx.Oms.Client.Desktop.Shared.Models;

public interface ICourierApi
{
    [Get("/api/v1/couriers/search")]
    Task<PagedResult<CourierDto>> SearchCouriers(
        [AliasAs("Page")] int page, 
        [AliasAs("PageSize")] int pageSize, 
        [AliasAs("SearchTerm")] string? searchTerm = null,
        [AliasAs("SortColumn")] string? sortColumn = null, 
        [AliasAs("SortOrder")] string? sortOrder = null);

    [Post("/api/v1/couriers")]
    Task<CourierDto> CreateCourier([Body] CreateCourierDto courier);
}
```

### Step 2: Create the ViewModel
Use **CommunityToolkit.Mvvm**. Inherit from `ObservableObject`.

**Key Practices**:
-   **Collections**: Use `ObservableCollection<T>`.
-   **Properties**: Use manual property syntax with `SetProperty` for `ObservableCollection` and complex types to ensure WinRT/AOT compatibility.
-   **Commands**: Use `IAsyncRelayCommand` for async actions (loading, saving).
-   **Navigation**: Implement `INavigationAware` if you need to load data when navigating to the page.

**File**: `Features/[Feature]/[Feature]ViewModel.cs`
```csharp
public partial class CouriersViewModel : ObservableObject, INavigationAware
{
    private readonly ICourierApi _api;
    
    // Manual property for WinRT compatibility
    private ObservableCollection<CourierDto> _items = new();
    public ObservableCollection<CourierDto> Items 
    { 
        get => _items; 
        set => SetProperty(ref _items, value); 
    }

    public IAsyncRelayCommand LoadDataCommand { get; }

    public CouriersViewModel(ICourierApi api)
    {
        _api = api;
        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
    }
    
    // ... LoadDataAsync logic ...
}
```

### Step 3: Create the View (Page)
Create a **WinUI 3 Page**.

**File**: `Features/[Feature]/[Feature]Page.xaml`
-   **Binding**: Use `x:Bind` for compiled bindings (better performance).
-   **Events**: For simple UI events like `QuerySubmitted` or `Sorting`, prefer **Code-Behind** handlers that call ViewModel commands, rather than complex arbitrary behaviors.
-   **DataGrid**: Use `CommunityToolkit.WinUI.UI.Controls.DataGrid`.

**Pagination Layout**:
Use a `Grid` for the footer:
-   **Left**: Page Size Selector (`ComboBox`).
-   **Center**: Summary Text ("Page 1 of X").
-   **Right**: Navigation Buttons (Prev/Next).

### Step 4: Register Services
Register your new types in the Dependency Injection container.

**File**: `App.xaml.cs`
```csharp
// In ConfigureServices method:
services.AddTransient<ICourierApi>(sp => RestService.For<ICourierApi>(...));
services.AddTransient<CouriersViewModel>();
services.AddTransient<CouriersPage>();
```

### Step 5: Add to Navigation
Expose the page in the main navigation menu.

**File**: `MainWindow.xaml` (or via `NavigationViewService` config)
```xml
<NavigationViewItem Content="Couriers" Tag="CouriersPage" Icon="Shop" />
```

## 3. UI Patterns

-   **Search**: Use `AutoSuggestBox` with `QuerySubmitted` event.
-   **Sorting**: Implement server-side sorting. Use `Tag` on DataGrid columns to map to API sort fields.
-   **No Data**: Always verify if the result count is 0 and show a friendly "No items found" message.
-   **Busy State**: Use `ProgressRing` bound to `IsLoading` property.

## 4. Add/Edit Dialogs (Recommended)
For creating or editing entities, we prefer **ContentDialogs** over full pages if the form is relatively simple.

**Pattern**:
1.  Create a separate UserControl or Page for the Form (e.g., `CourierForm.xaml`).
2.  Wrap it in a `ContentDialog` or simpler, just define the Form directly in the `ContentDialog.Content`.
3.  **Prevent Closing**: Handle the `Closing` event on the dialog. Set `args.Cancel = true` if the save operation fails or validation fails. Only allow close on success or cancellation.

### Read-Only / View Mode
To support "View Details" without duplicating forms:
1.  Add a `bool isReadOnly = false` parameter to the Dialog constructor.
2.  If true:
    -   Set `IsReadOnly = true` and `IsEnabled = false` on inputs.
    -   Hide the Primary ("Save") button (`PrimaryButtonText = ""`).
    -   Change the Title (e.g., "Courier Details").
3.  Add a **View Button** (Icon: `&#xE890;` or `&#xE7AD;`) to the DataGrid Actions column.

## 5. DataGrid Best Practices

To ensure a professional and responsive DataGrid:

### Layout & Scrolling
-   **Constrained Height**: The `DataGrid` must have a finite height to show scrollbars.
    -   **Approach A**: If using a `Grid` row with `Height="*"`, ensure the parent container (like `Frame`) does NOT have a `ScrollViewer` enabling infinite height. set `ScrollViewer.VerticalScrollBarVisibility="Disabled"` on the hosting Frame.
    -   **Approach B**: Wrap the DataGrid in a `Grid` and bind `Width/Height` to the container's `ActualWidth/ActualHeight` (less preferred but works for complex nesting).
-   **Scrollbars**: Explicitly set `VerticalScrollBarVisibility="Auto"` and `HorizontalScrollBarVisibility="Auto"`.

### Column Styling
-   **Alignment**: Use `ElementStyle` instead of full `CellTemplate` for simple text alignment.
    ```xml
    <toolkit:DataGridTextColumn Header="Phone" ElementStyle="{StaticResource RightAlignedCellStyle}" HeaderStyle="{StaticResource RightAlignedHeaderStyle}" />
    ```
    (These styles are available in `Shared/Styles/DataGrid.xaml`).
-   **Responsiveness**: Always set `MinWidth` (e.g., `100-150`) on columns to prevent crushing.
-   **Width Strategies**: Use `*` for main content (Name/Description) and `Auto` or fixed width for metadata (Status/dates).

### Visual Polish
-   **Status Columns**: Use `DataGridTemplateColumn` with a reusable `Badge` style (see `Shared/Styles/Badge.xaml`).
-   **Actions Column**: Center align action buttons using `HeaderStyle="{StaticResource CenteredHeaderStyle}"` and a centered `StackPanel`.
