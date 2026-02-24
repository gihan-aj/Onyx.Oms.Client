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

## 4. Add/Edit Forms (Page-Based Architecture)
For creating or editing entities, we use **dedicated Pages** (e.g., `[Feature]FormPage.xaml`) rather than `ContentDialog`s. 

This architectural decision is required because WinUI 3 strictly forbids opening two native `ContentDialog`s simultaneously. If a form is inside a dialog, our global `ProblemDetailsHandler` cannot show validation error dialogs or might obscure toast notifications behind the dialog backdrop.

**Pattern**:
1. Create a `[Feature]FormPage.xaml` for entry forms.
2. Wrap the form in a center-aligned elevated `Border` (Card style) inside a `ScrollViewer` to ensure the form remains accessible on small screens.
3. Use `INavigationService` to navigate to this page (passing an ID for edit mode, or null for create mode). Navigate `GoBack()` on success or cancellation.
4. Implement inline field validation by mapping `Refit.ApiException` errors to specific ViewModel properties if inline validation is desired.

### Read-Only / View Details Dialog
We use a **ContentDialog** for the "View Details" action because read-only views do not trigger validation error dialogs, avoiding the overlap issue.
1. Create a dedicated `[Feature]DetailsDialog.xaml`.
2. Keep it clean: use `TextBlock`s and visual elements instead of disabled `TextBox`es and `CheckBox`es.
3. Provide a single "Close" button.
4. Add a **View Button** (Icon: `&#xE890;`) to the DataGrid Actions column to trigger this dialog.

## 5. DataGrid Best Practices

To ensure a professional and responsive DataGrid:

### Layout & Scrolling
-   **Scrollable Pages**: Both list pages (containing the DataGrid) and form pages should be scrollable to support responsive resizing. Wrap the main content or the entire page in a `ScrollViewer` with `VerticalScrollBarVisibility="Auto"`.
-   **Constrained Height**: The `DataGrid` must have a finite height to show scrollbars. If the grid takes up the remaining space, ensure its bounding container provides a layout constraint.
-   **Scrollbars**: Explicitly set `VerticalScrollBarVisibility="Auto"` and `HorizontalScrollBarVisibility="Auto"` on the `DataGrid` itself.

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

## 6. Error Handling

API requests made via Refit clients are registered with the `ProblemDetailsHandler` in `App.xaml.cs`. This global handler natively intercepts `ProblemDetails` JSON responses (like 400 Bad Request or 409 Conflict) and standard HTTP errors.

**Best Practices:**
- Do **not** manually show notifications or error dialogs for API errors in your feature pages or ViewModels. The `ProblemDetailsHandler` automatically displays context-appropriate Toasts or Validation Dialogs globally.
- You do **not** need to handle validation errors manually unless you want to map them to inline field-specific errors.
- Use `try-catch` in your ViewModel **only** for local UI state cleanup (e.g., setting `IsLoading = false` in a `finally` block). 

**Example ViewModel Implementation:**
```csharp
try 
{
    IsBusy = true;
    await _api.CreateCourier(dto);
    _toastService.ShowSuccess("Success", "Courier created.");
    // Proceed to refresh grid or close dialog
}
catch (Exception)
{
    // The ProblemDetailsHandler has already shown the error UI.
    // Handle any local ViewModel state reset here, but do not show another error Toast.
}
finally
{
    IsBusy = false;
}
```

## 7. Permission-Based Access Control

We employ a robust, completely synchronized permission architecture to manage UI features dynamically without duplicating backend logic.

### Constants Map
All backend permission strings must be identically mapped in `Shared/Constants/Permissions.cs`. Do not hardcode raw strings in your code.
```csharp
public static class Permissions
{
    public static class Couriers
    {
        public const string Create = "Permissions.Couriers.Create";
        // ...
    }
}
```

### Accessing Permissions
The `IPermissionService` caches user permissions explicitly upon login. Use it synchronously within ViewModels or Code-Behind:
- `_permissionService.CanExecute(Permissions.Couriers.Create)`: For button-level checks.
- `_permissionService.HasFeatureAccess("Permissions.Couriers.")`: For Navigation block routing checks.

### UI Toggling Standards

**1. Page-Level Action Buttons**
For top-level Page actions (e.g., a "New Courier" button sitting above a DataGrid), handle the `Visibility` directly in the **Code-Behind**.
```csharp
// Inside FeaturePage.xaml.cs constructor
var permissionService = App.Current.Services.GetRequiredService<IPermissionService>();
NewCourierButton.Visibility = permissionService.CanExecute(Permissions.Couriers.Create) 
    ? Visibility.Visible : Visibility.Collapsed;
```

**2. DataGrid Row Actions (Edit/Delete)**
For action buttons residing *inside* a DataGrid `DataTemplate`, direct `x:Name` access from Code-Behind is impossible. Instead:
1. Add property flags to your `Dto` (e.g., `CourierDto.CanEdit`, `CourierDto.CanDelete`).
2. Populate these properties explicitly inside the `ViewModel.LoadDataAsync` method by querying the `IPermissionService` once.
3. In your XAML `DataTemplate`, bind the **`IsEnabled`** property of the Edit/Delete/Toggle buttons directly to these properties to maintain a consistent UI layout for all users, regardless of access level.
```xml
<Button Content="&#xE70F;" Style="{StaticResource SubtleButtonStyle}" 
        IsEnabled="{x:Bind CanEdit, Mode=OneWay}" />
```
