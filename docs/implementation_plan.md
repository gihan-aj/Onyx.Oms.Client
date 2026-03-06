# Goal Description

The feature to add dynamic specifications on a per-category basis requires an update to the new [CreateProductPage](file:///c:/Users/gihan/source/repos/Onyx.Oms.Client/src/Onyx.Oms.Client.Desktop/Features/Products/CreateProductPage.xaml.cs#8-31). We need to dynamically fetch the required fields (like "RAM" or "Screen Size") when a Category is selected and render them as native WinUI input fields (TextBox, ComboBox, CheckBox). When saving the product, we collect the entered values into a key-value dictionary to send to the backend.

## Proposed Changes

### [IProductCategoryLookupApi]

Add the newly defined `GetCategoryById` endpoint and its required DTO models:
- Add `Task<ProductCategoryResponse> GetCategoryById(Guid id, [AliasAs("includeParentSpecs")] bool includeParentSpecs);`
- Define [ProductCategoryResponse](file:///c:/Users/gihan/source/repos/Onyx.Oms.Client/src/Onyx.Oms.Client.Desktop/Features/Products/IProductCategoryLookupApi.cs#42-47) and [SpecDefinition](file:///c:/Users/gihan/source/repos/Onyx.Oms.Client/src/Onyx.Oms.Client.Desktop/Features/Products/IProductCategoryLookupApi.cs#48-56) / `SpecType` DTO models.

### [CreateProductViewModel]

- Update the setter of `SelectedCategory` to trigger an asynchronous fetch for the category's specifications.
- Add a new collection: `public ObservableCollection<SpecFieldViewModel> DynamicSpecs { get; } = new();`.
- Implement a method [LoadCategorySpecificationsAsync(Guid categoryId)](file:///c:/Users/gihan/source/repos/Onyx.Oms.Client/src/Onyx.Oms.Client.Desktop/Features/Products/CreateProductViewModel.cs#168-199) to populate `DynamicSpecs` based on the fetched `AllSpecifications`.
- In [SaveAsync](file:///c:/Users/gihan/source/repos/Onyx.Oms.Client/src/Onyx.Oms.Client.Desktop/Features/Products/CreateProductViewModel.cs#314-399), pack the populated `DynamicSpecs` into the [Specifications](file:///c:/Users/gihan/source/repos/Onyx.Oms.Client/src/Onyx.Oms.Client.Desktop/Features/Products/CreateProductViewModel.cs#168-199) `Dictionary<string, string>` for the [CreateProductCommand](file:///c:/Users/gihan/source/repos/Onyx.Oms.Client/src/Onyx.Oms.Client.Desktop/Features/Products/CreateProductCommand.cs#6-22).

### [CreateProductPage]

- **XAML View**: Add a new "Specifications" section.
    - Define `DataTemplate` items in `Page.Resources` for `TextSpecTemplate`, `SelectSpecTemplate`, `ToggleSpecTemplate`.
    - Define the `local:FormTemplateSelector` to choose the correct template.
    - Insert an `ItemsControl` bound to `ViewModel.DynamicSpecs` to render the dynamic fields.
- **FormTemplateSelector.cs**: Create this new class to act as the `DataTemplateSelector` for the dynamic fields as specified in the ADR docs.
- **SpecFieldViewModel.cs**: Create or add this wrapper class to store the definition plus the bound user `Value`. 

### [Upload Images Feature]

- **File Picker & View**: In [CreateProductPage.xaml](file:///c:/Users/gihan/source/repos/Onyx.Oms.Client/src/Onyx.Oms.Client.Desktop/Features/Products/CreateProductPage.xaml), add an "Images" section with an "Upload Image" button and an `ItemsControl` or `GridView` to display existing images. In [CreateProductPage.xaml.cs](file:///c:/Users/gihan/source/repos/Onyx.Oms.Client/src/Onyx.Oms.Client.Desktop/Features/Products/CreateProductPage.xaml.cs), implement the `FileOpenPicker` (initializing with the Window handle) to let the user select image files.
- **ViewModel Logic**: Create an `AddImageAsync` command/method in [CreateProductViewModel](file:///c:/Users/gihan/source/repos/Onyx.Oms.Client/src/Onyx.Oms.Client.Desktop/Features/Products/CreateProductViewModel.cs#13-430). It will receive the file stream from the View.
    - Generate a unique file name using `BaseSku` (if available) and a GUID.
    - Call `IFileService.SaveImageAsync("ProductDrafts", generatedName, stream)` to resize (max 800px) and save the file locally.
    - Call `IFileService.ReadFileAsync` to load the saved file into a `byte[]` to create a `BitmapImage` in memory. This prevents file locking so the user can delete it later.
    - Add the image to the `Images` observable collection ([CreateProductImageDto](file:///c:/Users/gihan/source/repos/Onyx.Oms.Client/src/Onyx.Oms.Client.Desktop/Features/Products/CreateProductCommand.cs#46-53) wrapper). If it's the first image, mark `IsMain = true`.
- **Deletion handling**: Add a `RemoveImageCommand` that removes the image from the collection and calls `IFileService.DeleteFileAsync`.

## Verification Plan

### Manual Verification
1. Run the application and open the "Create New Product" page.
2. Select a category that has known specifications (e.g., "Smartphones").
3. Verify that the correct dynamically generated fields appear on the screen with the correct input types.
4. Fill out the fields and confirm via logs or debugger that the [CreateProductCommand](file:///c:/Users/gihan/source/repos/Onyx.Oms.Client/src/Onyx.Oms.Client.Desktop/Features/Products/CreateProductCommand.cs#6-22) packages the [Specifications](file:///c:/Users/gihan/source/repos/Onyx.Oms.Client/src/Onyx.Oms.Client.Desktop/Features/Products/CreateProductViewModel.cs#168-199) dictionary correctly.
5. Upload an image. Verify it displays in the UI.
6. Verify the physical file is created in the local AppData folder.
7. Delete the image from the UI and verify it is removed from the screen and the local folder without throwing an "in use" exception.
