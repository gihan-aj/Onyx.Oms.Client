# System Configuration & Tenant Management Architecture - Onyx.Oms

## 1. Architectural Overview

The Onyx.Oms system separates configuration data into two distinct architectural buckets to optimize database performance and maintain clean domain boundaries:

1. **Tenant Profile (Configuration Data):** Slow-changing data that defines the identity and core operational rules of the business (e.g., Store Name, Base Currency, Address). Read frequently, updated rarely.
2. **App Sequences (Operational Data):** Fast-changing data that governs system operations (e.g., Next Order Number, Next SKU). Requires atomic updates and strict concurrency control to prevent duplicate values. Handled by a dedicated Vertical Slice.



## 2. Domain Entities (Backend)

The `TenantProfile` entity acts as the single source of truth for global business rules. It utilizes strongly typed objects for core properties and a JSON column for loose UI preferences to prevent database schema bloat.

```csharp
public class TenantProfile : AuditableEntity<Guid>
{
    private TenantProfile() { } // EF Core requirement

    public TenantProfile(
        Guid id, 
        string storeName, 
        string contactEmail, 
        string baseCurrency = "LKR", 
        string weightUnit = "kg") : base(id)
    {
        StoreName = storeName;
        ContactEmail = contactEmail;
        BaseCurrency = baseCurrency.ToUpperInvariant();
        WeightUnit = weightUnit.ToLowerInvariant();
        PreferencesJson = "{}"; 
    }

    // Identity & Contact
    public string StoreName { get; private set; }
    public string? LegalName { get; private set; }
    public string? TaxRegistrationNumber { get; private set; }
    public string ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }

    // Physical Location (Assumes an Address Value Object exists)
    public Address? StoreAddress { get; private set; }

    // Regional & Units (The Single Source of Truth)
    public string BaseCurrency { get; private set; }
    public string WeightUnit { get; private set; }

    // Invoicing & Documents
    public string? InvoiceFooterText { get; private set; }
    public string? LogoUrl { get; private set; }

    // Loose UI Preferences (Serialized JSON)
    public string PreferencesJson { get; private set; }

    // --- Domain Behaviors ---

    public void UpdateStoreInfo(string storeName, string legalName, string taxId, string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(storeName)) 
            throw new ArgumentException("Store Name is required.");

        StoreName = storeName;
        LegalName = legalName;
        TaxRegistrationNumber = taxId;
        ContactEmail = email;
        ContactPhone = phone;
    }

    public void UpdateRegionalSettings(string currency, string weightUnit)
    {
        if (string.IsNullOrWhiteSpace(currency) || string.IsNullOrWhiteSpace(weightUnit))
            throw new ArgumentException("Currency and Weight Unit are required.");

        BaseCurrency = currency.ToUpperInvariant();
        WeightUnit = weightUnit.ToLowerInvariant();
    }

    public void UpdateAddress(Address address) => StoreAddress = address;

    public void UpdatePreferences(string jsonFormattedPreferences)
    {
        // Optional: Add JSON validation logic here
        PreferencesJson = jsonFormattedPreferences;
    }
}
```

## 3. Entity Framework Core Configuration

The configuration maps the Value Object (`Address`) and ensures the JSON preferences column is handled correctly.

```csharp
public class TenantProfileConfiguration : IEntityTypeConfiguration<TenantProfile>
{
    public void Configure(EntityTypeBuilder<TenantProfile> builder)
    {
        builder.ToTable("TenantProfiles");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.StoreName).IsRequired().HasMaxLength(200);
        builder.Property(t => t.BaseCurrency).IsRequired().HasMaxLength(3);
        builder.Property(t => t.WeightUnit).IsRequired().HasMaxLength(10);
        
        // Maps the JSON string directly. 
        builder.Property(t => t.PreferencesJson).HasColumnType("jsonb"); // Use "nvarchar(max)" for SQL Server

        // Map the Address Value Object
        builder.ComplexProperty(t => t.StoreAddress, ab => 
        {
            ab.IsRequired(false);
            ab.Property(a => a.Street).HasColumnName("AddressStreet").HasMaxLength(200);
            ab.Property(a => a.City).HasColumnName("AddressCity").HasMaxLength(100);
            ab.Property(a => a.State).HasColumnName("AddressState").HasMaxLength(100);
            ab.Property(a => a.PostalCode).HasColumnName("AddressPostalCode").HasMaxLength(20);
            ab.Property(a => a.Country).HasColumnName("AddressCountry").HasMaxLength(100);
        });
    }
}
```

## 4. API Integration & Tenant Identification

To bridge the gap between the current single-store local deployment and future SaaS aspirations without over-engineering, the system uses **Claims-Based Routing** for configuration data.



### 4.1 The `tenant_id` JWT Claim
* The WinUI 3 client does not know its own Tenant ID and never passes it in the URL. It simply executes a parameterless request: `GET /api/tenant/profile`.
* The Identity Provider (Onyx.IdP via OpenIddict) injects a custom `tenant_id` claim into the user's JWT bearer token upon login.
* **Current Phase (Local Deployment):** The backend Vertical Slice Query Handler extracts the claim from the `HttpContext`. If the claim is missing, it safely falls back to querying the first and only row in the `TenantProfiles` table.
* **Future Phase (SaaS):** The backend will strictly enforce the `tenant_id` claim to securely isolate store data. Because the routing relies entirely on the token payload, this transition will require zero changes to the WinUI client.

## 5. Client Global State Management (`ITenantContextService`)

To avoid querying the database for every currency symbol render, the WinUI 3 client caches this configuration on startup.

### 5.1 Responsibilities
* **Initialization:** Fetches the `TenantProfile` from the API during the app's splash screen or initial load.
* **Global Access:** Exposes properties like `BaseCurrency` and `WeightUnit` as an injected Singleton service.
* **UI Binding:** ViewModels read from `_tenantContext.BaseCurrency` to display labels (e.g., "LKR") next to purely numeric TextBoxes, ensuring the user never types the unit manually.

## 6. UI/UX Organization: The Settings Interface

The Settings page must not be a monolithic scrolling form. It utilizes a `NavigationView` (left-aligned sidebar) or a `TabView` to compartmentalize configuration into distinct operational areas.

### 6.1 Category Layouts
* **General (Store Profile):** * Fields: Store Name, Legal Name, Tax ID, Contact Details.
  * Address entry form.
  * Branding: File picker for `LogoUrl`.
* **Regional & Financial:**
  * Dropdowns for `BaseCurrency` and `WeightUnit`.
  * *UX Rule:* Display a prominent `InfoBar` warning that changing the currency does not retroactively convert historical order financial records.
* **Numbering & Sequences (`AppSequences` Slice):**
  * Displays a list/grid of active sequences (e.g., `OrderNumber`, `ProductSku`).
  * Allows updating the `Prefix` and `Next Value`.
  * *UX Rule:* Include validation warnings if a user attempts to lower a sequence number, which could cause primary key/unique constraint collisions.
* **Printing & Documents:**
  * Rich text or multiline TextBox for `InvoiceFooterText`.
  * UI Toggles for loose preferences (e.g., "Auto-print waybill on Pack"), which are serialized and saved into the `PreferencesJson` property.