# Product Category & Dynamic Specifications - Onyx.Oms

## 1. Architectural Decision Record (ADR)

### 1.1 Context
In a diverse e-commerce system, different product categories require fundamentally different attributes. A "T-Shirt" needs `Size` and `Fabric`, while a "Laptop" needs `Processor` and `RAM`.
* **Old Approach:** Hard-coding columns (`HasColor`, `HasSize`) limits the system to clothing only.
* **Naive Approach:** Adding hundreds of nullable columns (`Column1`, `Column2`) creates a sparse, unmaintainable database.

### 1.2 Decision
We will implement a **Hybrid EAV (Entity-Attribute-Value)** model using **JSON Storage**.
* **Category Entity:** Stores the *Definition* of what attributes are required (Metadata).
* **Product Entity:** Stores the *Values* for those attributes (Data).

### 1.3 Benefits
* **Zero Schema Migrations:** Marketing can invent a new attribute (e.g., "Eco-Friendly Rating") and add it to a category instantly without a developer modifying the SQL schema.
* **Strong Validation:** Unlike a raw "property bag," the Category entity enforces strict data types (e.g., ensuring "Ram" is a number, not text).
* **Clean UI:** The UI dynamically generates form fields based on the category, ensuring users only see relevant inputs.

---

## 2. Domain Logic & Entities

### 2.1 The Definition Object (Value Object)
This simple class acts as the blueprint for a single form field. It is **not** an entity; it is serialized as JSON inside the `ProductCategory` table.

```csharp
public enum SpecType
{
    Text,       // Simple TextBox
    Number,     // NumberBox with validation
    Select,     // ComboBox (Single Selection)
    MultiSelect,// Tokenized Input (Tags)
    Toggle,     // CheckBox
    Date        // DatePicker
}

public class SpecDefinition
{
    public string Key { get; set; } = string.Empty;       // Internal ID: "screen_size"
    public string Label { get; set; } = string.Empty;     // Display: "Screen Size (inches)"
    public SpecType Type { get; set; } = SpecType.Text;
    public bool IsRequired { get; set; } = false;
    
    // For 'Select' types only. e.g. ["4GB", "8GB", "16GB"]
    public List<string> Options { get; set; } = new();
}
```

### 2.2 The Category Entity (The Rules Engine)
The category acts as the "Factory" for product forms. It dictates what a product *must* look like.

```csharp
public class ProductCategory : AuditableEntity<Guid>
{
    // ... Existing Properties ...

    // Stored as JSON: [{"Key": "material", "Type": "Text"}, {"Key": "gender", "Type": "Select"}]
    private readonly List<SpecDefinition> _specifications = new();
    public virtual IReadOnlyCollection<SpecDefinition> Specifications => _specifications.AsReadOnly();

    public void UpdateSpecifications(List<SpecDefinition> newSpecs)
    {
        // Business Logic: Prevent Duplicate Keys
        var duplicateKeys = newSpecs.GroupBy(x => x.Key).Where(g => g.Count() > 1).Select(y => y.Key);
        if (duplicateKeys.Any())
            throw new DomainException($"Duplicate specification keys: {string.Join(", ", duplicateKeys)}");

        // Business Logic: Sanity Check
        if (newSpecs.Count > 30)
            throw new DomainException("Categories cannot have more than 30 specifications.");

        _specifications.Clear();
        _specifications.AddRange(newSpecs);
    }
}
```

---

## 3. UI Implementation (WinUI 3 Dynamic Form)

The UI does not hard-code these fields. Instead, it uses an `ItemsControl` bound to a collection of ViewModels. A `DataTemplateSelector` automatically picks the correct input control (TextBox, ComboBox, etc.) at runtime.

### 3.1 The View Models
These are lightweight wrappers used only for the UI binding.

```csharp
// Represents a single field on the screen
public class SpecFieldViewModel : ObservableObject
{
    public string Key { get; set; }
    public string Label { get; set; }
    public SpecType Type { get; set; }
    public bool IsRequired { get; set; }
    public ObservableCollection<string> Options { get; set; }

    // THE USER'S INPUT binds here
    [ObservableProperty] private string _value; 
}
```

### 3.2 The Template Selector (C#)
This class tells the UI: *"If the type is 'Select', use the ComboBox template. Otherwise, use the TextBox template."*

```csharp
public class FormTemplateSelector : DataTemplateSelector
{
    public DataTemplate TextTemplate { get; set; }
    public DataTemplate SelectTemplate { get; set; }
    public DataTemplate ToggleTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is SpecFieldViewModel vm)
        {
            return vm.Type switch
            {
                SpecType.Select => SelectTemplate,
                SpecType.Toggle => ToggleTemplate,
                _ => TextTemplate
            };
        }
        return TextTemplate;
    }
}
```

### 3.3 The XAML (The Form Generator)
This is all the code needed to generate a form with 1 field or 100 fields.

```xml
<Page.Resources>
    <DataTemplate x:Key="TextSpecTemplate" x:DataType="vm:SpecFieldViewModel">
        <TextBox Header="{x:Bind Label}" 
                 Text="{x:Bind Value, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" 
                 PlaceholderText="Enter value..." />
    </DataTemplate>

    <DataTemplate x:Key="SelectSpecTemplate" x:DataType="vm:SpecFieldViewModel">
        <ComboBox Header="{x:Bind Label}" 
                  ItemsSource="{x:Bind Options}" 
                  SelectedItem="{x:Bind Value, Mode=TwoWay}"
                  HorizontalAlignment="Stretch" />
    </DataTemplate>

    <DataTemplate x:Key="ToggleSpecTemplate" x:DataType="vm:SpecFieldViewModel">
        <CheckBox Content="{x:Bind Label}" 
                  IsChecked="{x:Bind Value, Mode=TwoWay, Converter={StaticResource StringToBoolConverter}}" />
    </DataTemplate>

    <local:FormTemplateSelector x:Key="SpecSelector"
                                TextTemplate="{StaticResource TextSpecTemplate}"
                                SelectTemplate="{StaticResource SelectSpecTemplate}"
                                ToggleTemplate="{StaticResource ToggleSpecTemplate}" />
</Page.Resources>

<ItemsControl ItemsSource="{x:Bind ViewModel.DynamicSpecs}"
              ItemTemplateSelector="{StaticResource SpecSelector}">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <StackPanel Spacing="16" />
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
</ItemsControl>
```

---

## 4. Data Flow Example

1.  **User Action:** Selects category **"Smartphones"**.
2.  **System Action:**
    * Loads `Category.Specifications` JSON: `[{"Key": "ram", "Type": "Select", "Options": ["8GB", "16GB"]}, {"Key": "screen", "Type": "Text"}]`
    * ViewModel creates 2 `SpecFieldViewModel` objects.
3.  **UI Rendering:**
    * Field 1 sees `Type=Select` $\rightarrow$ Renders a **ComboBox** with "8GB" and "16GB".
    * Field 2 sees `Type=Text` $\rightarrow$ Renders a **TextBox**.
4.  **Saving:**
    * ViewModel loops through the list.
    * Extracts values: `{"ram": "16GB", "screen": "OLED 6 inch"}`.
    * Saves this dictionary to the `Product` entity.