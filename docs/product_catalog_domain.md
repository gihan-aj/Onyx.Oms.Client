# Product Catalog Architecture & Business Logic - Onyx.Oms (v3.1)

## 1. Domain Architecture (Backend)

The Product Catalog uses a **Dynamic Matrix** architecture within Domain-Driven Design (DDD). The `Product` acts as the Aggregate Root, managing a flexible schema of Options (Axes) and controlling the lifecycle of `ProductVariant` and `ProductImage` entities.

### 1.1 Value Objects & Nullability
* **Money:** Immutable record. Always required (Price cannot be null).
* **Weight? (Nullable):**
    * **Value:** `null` represents a **Non-Physical/Digital Product** (Shipping logic is skipped).
    * **Value:** `0` represents a **Physical Product** with negligible weight (e.g., Free Shipping).
    * **Implementation:** EF Core maps this to nullable columns (`WeightValue`, `WeightUnit`). If `Weight` is null, both columns are stored as `NULL`.

### 1.2 Core Domain Rules

#### A. The "Dynamic Options" Pattern
Instead of hardcoded `HasColor` / `HasSize` flags, the Product defines its own **Option Axes** via a JSON Value Object.
* **Structure:** `Product.Options` = `[{ "Name": "Color", "Values": ["Red", "Blue"] }, { "Name": "Size", "Values": ["M", "L"] }]`.
* **Flexibility:** Allows for any variation type (e.g., "Fabric", "Sleeve Type", "Memory").
* **Validation:** The Product entity enforces that all Variants strictly adhere to this schema. A Variant cannot have an attribute "Fabric" if the Product doesn't define that Option.

#### B. Variant Identity (Immutable Attributes)
* **The Rule:** A Variant's attributes (e.g., "Red - Large") are its **Identity**. They are **Immutable**.
* **Logic:** You cannot "edit" a variant from "Red" to "Green". You must **Soft Delete** the "Red" variant and create a new "Green" variant.
* **Benefit:** Preserves historical order data integrity. If a customer bought a "Red" item 2 years ago, that record remains accurate even if "Red" is discontinued.

#### C. Smart Matrix Reconciliation (Cascading Deletes)
Updating the Product's `Options` triggers a "Safe Reconciliation" process:
* **Additive Changes (Safe):** Adding a new value (e.g., "Green") is always allowed.
* **Subtractive Changes (Conditional):** Removing a value (e.g., "Red") automatically **Soft Deletes** all active variants that rely on "Red". It does *not* block the user.
* **Structural Changes (Blocked):** Removing an entire Axis (e.g., deleting "Size") is **blocked** if *any* active variants exist, as this creates data collisions (duplicate SKUs).

#### D. Image-Option Linking
* **Logic:** Images are not linked to specific `VariantId`s (which causes duplication). Instead, they are linked to **Option Values**.
* **Example:** An image tagged with `Option: "Color", Value: "Red"` will automatically appear for *all* Red variants (Red-S, Red-M, Red-L).

### 1.3 Variant-Less Products (Default Variant Pattern)

Some products have no meaningful variations (e.g., a single-model USB cable or a book). For these, defining a variant matrix is unnecessary overhead.

#### The Design
* **`Product.HasVariants`** (bool) is the single flag that drives this:
  * `true` → Standard variant-matrix mode. The user defines Options and generates variants from them.
  * `false` → Simple logistics mode. The UI hides the options/variant section and exposes plain SKU, price, cost, weight, and stock fields.
* **Internally**, a single **default variant** with an empty `Attributes` list (`[]`) is always created and maintained. There is no special table or flag on the variant itself — it is simply a variant that happens to have no attributes.
* **`Product.DefaultVariant`** accessor: returns the active default variant when `HasVariants = false`, or `null` when the product uses the variant matrix.

#### Lifecycle
| Operation | Variant-mode (`HasVariants = true`) | Simple-mode (`HasVariants = false`) |
|---|---|---|
| Create product | Pass `options` → auto-derives `HasVariants = true` | Pass no options → `HasVariants = false`, default variant created automatically |
| Update logistics | `ProductVariant.UpdateLogistics()` per variant | `Product.SetDefaultVariantLogistics(sku, cost, price, weight, stock)` |
| Add variant | `Product.AddVariant()` | Blocked — returns domain error |

#### Order Fulfilment (Transparent)
This distinction is **UI-only**. The order fulfilment pipeline always works with `ProductVariant` records regardless of `HasVariants`. A variant-less product simply has exactly one `ProductVariant` whose `Attributes` list is empty.

### 1.4 Command Handler Validations
* **SKU Uniqueness:** The handler enforces global uniqueness for `BaseSku` and `Variant.Sku`.
* **Attribute Validation:** Before creating a variant (*only when `HasVariants = true`*), the handler (or Domain Factory) verifies:
    1.  **Count:** Variant has exactly the same number of attributes as Product Options.
    2.  **Match:** Attribute names match Option names.
    3.  **Validity:** Attribute values exist in the Option's allowed values list.

---

## 2. Final Domain Entities

```csharp
// Value Objects
public record Money(decimal Amount, string Currency);
public record Weight(decimal Value, string Unit);
public class ProductOption { public string Name { get; set; } public List<string> Values { get; set; } }
public class VariantAttribute { public string Name { get; set; } public string Value { get; set; } }

// 1. PRODUCT (Aggregate Root)
public class Product : AuditableEntity<Guid>
{
    // Core Data
    public string Name { get; private set; }
    public string BaseSku { get; private set; } 
    public Guid CategoryId { get; private set; }
    
    // Financials (used as defaults when creating new variants)
    public Money BaseCost { get; private set; }
    public Money BasePrice { get; private set; }
    public Weight? BaseWeight { get; private set; } // Nullable = Digital/Service

    // Variant Mode
    public bool HasVariants { get; private set; }
    public ProductVariant? DefaultVariant => HasVariants ? null : _variants.FirstOrDefault(v => !v.IsDeleted && !v.Attributes.Any());

    // DYNAMIC SCHEMA (Stored as JSON)
    private readonly List<ProductOption> _options = new();
    public IReadOnlyCollection<ProductOption> Options => _options.AsReadOnly();

    // COLLECTIONS
    private readonly List<ProductVariant> _variants = new();
    public virtual IReadOnlyCollection<ProductVariant> Variants => _variants.AsReadOnly();
    
    private readonly Dictionary<string, string> _specifications = new();
    public IReadOnlyDictionary<string, string> Specifications => _specifications.AsReadOnly();

    // LOGIC: Create (HasVariants is auto-derived from whether options are provided)
    public static Result<Product> Create(string name, string baseSku, ..., List<ProductOption>? options = null)
    {
        bool hasVariants = options != null && options.Count > 0;
        var product = new Product(..., hasVariants);
        
        if (hasVariants)
            product._options.AddRange(options!);
        else
            product._variants.Add(ProductVariant.CreateDefault(product, baseSku, baseCost, basePrice, baseWeight).Value);
            
        return Result.Success(product);
    }

    // LOGIC: Simple logistics update for variant-less products
    public Result SetDefaultVariantLogistics(string sku, Money cost, Money price, Weight? weight, int stockOnHand);

    // LOGIC: Smart Reconciliation (only when HasVariants = true)
    public Result UpdateOptionValues(List<ProductOption> newOptions, string userId);
    
    // LOGIC: Specification Validation
    public Result UpdateSpecifications(Dictionary<string, string> newSpecs, ProductCategory category);

    // LOGIC: Variant Collision Check (blocked when HasVariants = false)
    public Result AddVariant(ProductVariant variant);
}

// 2. PRODUCT VARIANT
public class ProductVariant : AuditableEntity<Guid>, ISoftDeletable
{
    public string Sku { get; private set; }
    
    // IMMUTABLE ATTRIBUTES (JSON)
    // Identity: "Color: Red", "Size: L"  — empty list for default/variant-less products
    private readonly List<VariantAttribute> _attributes = new();
    public IReadOnlyCollection<VariantAttribute> Attributes => _attributes.AsReadOnly();

    // Logistics (Mutable)
    public Money Cost { get; private set; }
    public Money Price { get; private set; }
    public Weight? Weight { get; private set; }
    public int StockOnHand { get; private set; }

    // Soft Delete
    public bool IsDeleted => DeletedAtUtc is not null;
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    // Factory: validated variant (requires product.HasVariants = true)
    public static Result<ProductVariant> Create(Product product, string sku, List<VariantAttribute> attributes, ...);
    
    // Factory: default variant for variant-less products (empty attributes, internal)
    internal static Result<ProductVariant> CreateDefault(Product product, string sku, Money cost, Money price, Weight? weight, int stockOnHand = 0);

    // Update path for normal variants
    public Result UpdateLogistics(Money baseCost, Money basePrice, Weight? baseWeight, Money? variantCost, Money? variantPrice, Weight? variantWeight);
    
    // Update path for the default variant (called via Product.SetDefaultVariantLogistics)
    internal Result UpdateDefaultLogistics(string sku, Money cost, Money price, Weight? weight, int stockOnHand);
}

// 3. PRODUCT IMAGE
public class ProductImage : Entity<Guid>
{
    public string Url { get; private set; }
    public bool IsMain { get; private set; }
    
    // Smart Linking
    public string? OptionName { get; private set; }  // e.g. "Color"
    public string? OptionValue { get; private set; } // e.g. "Red"

    public Result LinkToOption(string optionName, string value, IReadOnlyCollection<ProductOption> validOptions);
}
```

---

## 3. EF Core Configuration (EF Core 10)

```csharp
// ProductConfiguration
public void Configure(EntityTypeBuilder<Product> builder)
{
    // Money — non-nullable complex type → inline columns
    builder.ComplexProperty(p => p.BasePrice, pb => { ... });
    builder.ComplexProperty(p => p.BaseCost, pb => { ... });

    // Weight — nullable → OwnsOne (ComplexProperty cannot be null in EF)
    builder.OwnsOne(p => p.BaseWeight, wb => { ... });

    // Options — List<ProductOption> → JSON column via ToJson()
    builder.OwnsMany(p => p.Options, ob => ob.ToJson());

    // Tags — List<string> primitive collection → JSON array column
    builder.PrimitiveCollection(p => p.Tags).HasColumnType("nvarchar(max)");

    // Specifications — Dictionary<string, string> → HasConversion to JSON string
    builder.Property(p => p.Specifications)
        .HasConversion(v => JsonSerializer.Serialize(v, ...), v => JsonSerializer.Deserialize<Dictionary<string,string>>(v, ...) ?? new())
        .HasColumnType("nvarchar(max)");
}

// ProductVariantConfiguration
public void Configure(EntityTypeBuilder<ProductVariant> builder)
{
    // Money — ComplexProperty (non-nullable)
    builder.ComplexProperty(v => v.Price, pb => { ... });
    builder.ComplexProperty(v => v.Cost, pb => { ... });

    // Weight — OwnsOne (nullable)
    builder.OwnsOne(v => v.Weight, wb => { ... });

    // Attributes — List<VariantAttribute> → JSON column; empty list "[]" for default variant
    builder.OwnsMany(v => v.Attributes, ab => ab.ToJson());

    // Soft Delete
    builder.HasQueryFilter(v => v.DeletedAtUtc == null);
}
```

---

## 4. Client Architecture & UX (WinUI 3)

### 4.1 The "Smart Matrix" Generator
* **Product Create:**
    1.  User defines Options (e.g., adds "Color" -> "Red, Blue").
    2.  User clicks **"Generate Variants"**.
    3.  System computes the Cartesian product (Red-S, Red-M...) and populates a DataGrid.
    4.  User edits Prices/Stock in the grid before saving.
* **Product Edit:**
    * **Options Section:** Adding values (e.g., "Green") is allowed. Removing values (e.g., "Red") shows a warning: *"Removing Red will archive 5 existing variants."*
    * **Regenerate:** Clicking "Update Matrix" adds the new "Green" rows to the grid without touching the existing "Blue" rows.

### 4.2 Variant-Less Toggle
* **UI:** A Toggle/Checkbox *"This product has variants"* is shown at product creation.
* **Logic:**
    * **On (HasVariants = true):** Options panel and variant DataGrid are visible. The user defines options and generates the matrix.
    * **Off (HasVariants = false):** Options panel and variant DataGrid are hidden. Simple fields for SKU, Price, Cost, Weight, and Stock are shown inline.
* **Transition:** Once a product is created with `HasVariants = true` and variants exist, the toggle cannot be turned off without deleting all variants first (and vice versa).

### 4.3 Physical vs. Digital Toggle
* **UI:** A Checkbox "This is a physical product" controls the `Weight` input.
* **Logic:**
    * **Checked:** `Weight` input is visible and required (> 0).
    * **Unchecked:** `Weight` input is hidden. ViewModel sends `null` to the backend.

### 4.4 Image Tagging
* **UI:** When uploading an image, a dropdown appears: *"Apply to..."*
* **Options:** `All Variants` (default), or specific Option Values (e.g., `Color: Red`).
* **Logic:** This writes to the `OptionName`/`OptionValue` columns on the Image entity.