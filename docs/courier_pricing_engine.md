# Feature Specification: Dynamic Courier Pricing Engine

## 1. Overview
The Onyx OMS implements a dynamic, math-based pricing engine for calculating courier shipping fees. 

Because Sri Lankan courier services (e.g., Domex, Koombiyo, Certis) frequently change their rates and geographic zone boundaries (e.g., "Colombo", "Greater Colombo", "Outstation"), hardcoding district-to-zone mappings is highly brittle. Instead, this engine relies on configurable **Zone Rates** that map directly to standard mathematical courier models.

## 2. Mathematical Model
Almost all local couriers utilize the following formula:
**Final Fee = Base Fee + Excess Weight Surcharge + COD Surcharge**

* **Base Weight Allowance:** The weight included in the base price (usually 1kg).
* **Base Fee:** The cost for that base allowance.
* **Excess Rate Per Kg:** The cost for every additional kilo (rounded up).
* **COD Surcharge:** A percentage of the Cash-on-Delivery amount collected.

## 3. Domain Architecture

To support this, the `Courier` entity contains a collection of `CourierZoneRate` entities. 

### The `CourierZoneRate` Entity
This entity acts as the configuration matrix for a specific geographical zone.

```csharp
public class CourierZoneRate : Entity<Guid>
{
    public Guid CourierId { get; private set; }
    public string ZoneName { get; private set; } // e.g., "Colombo 1-15", "Outstation"
    
    // Financials
    public Money BaseFee { get; private set; } 
    public decimal BaseWeightKg { get; private set; } 
    public Money ExcessFeePerKg { get; private set; } 
    public decimal CodPercentage { get; private set; } 

    // Dynamic Mapping
    public bool IsDefault { get; private set; } 
    
    private readonly List<string> _coveredDistricts = new();
    public IReadOnlyCollection<string> CoveredDistricts => _coveredDistricts.AsReadOnly();
}
```

## 4. Core Workflows & Logic

### 4.1 Zone Resolution
When an order is prepared for shipping, the system must determine which rate applies based on the order's `ShippingAddress.District`. It uses a **Specific Match -> Default Fallback** pattern.

```csharp
public CourierZoneRate GetApplicableRate(Courier courier, string targetDistrict)
{
    // 1. Find explicit mapping
    var specificZone = courier.ZoneRates
        .FirstOrDefault(z => !z.IsDefault && 
                             z.CoveredDistricts.Contains(targetDistrict, StringComparer.OrdinalIgnoreCase));

    if (specificZone != null) return specificZone;

    // 2. Fallback to default (e.g., "Outstation")
    var defaultZone = courier.ZoneRates.FirstOrDefault(z => z.IsDefault);
    if (defaultZone != null) return defaultZone;

    throw new InvalidOperationException($"No shipping rate configured for district '{targetDistrict}'.");
}
```

### 4.2 Fee Calculation Engine
Once the correct `CourierZoneRate` is identified, the engine calculates the exact fee using the order's total weight and COD requirement.

```csharp
public decimal CalculateShippingFee(CourierZoneRate rate, decimal totalOrderWeightKg, decimal totalCodAmount)
{
    decimal shippingFee = rate.BaseFee;

    // Calculate Excess Weight (Rounded up to nearest whole integer)
    if (totalOrderWeightKg > rate.BaseWeightKg)
    {
        decimal excessWeight = Math.Ceiling(totalOrderWeightKg - rate.BaseWeightKg);
        shippingFee += (excessWeight * rate.ExcessFeePerKg);
    }

    // Calculate COD Surcharge
    if (totalCodAmount > 0 && rate.CodPercentage > 0)
    {
        shippingFee += (totalCodAmount * (rate.CodPercentage / 100m));
    }

    return shippingFee;
}
```

## 5. UI/UX Guidelines
* **Auto-Seeding:** When a user creates a new Courier, the system should automatically seed two zones: "Colombo" (CoveredDistricts: Colombo) and "Outstation" (IsDefault: true). This provides a zero-configuration starting point.
* **Editable Output:** When calculating the fee during the `Ship` workflow, the calculated value must populate a standard `NumberBox`. This value **must remain editable** by the user to allow for manual overrides (e.g., waiving shipping fees for loyal customers).
* **District Mapping:** In the Courier Management UI, zones are managed using a multi-select token box containing all 25 Sri Lankan districts, preventing manual typo errors.

## 6. Architectural Benefits
1.  **Zero Hardcoding:** No geographic logic exists in the compiled C# code.
2.  **Fault Tolerant:** The `IsDefault` flag ensures the system never crashes if a new district is added or a user forgets to map a region.
3.  **Future-Proof:** If a courier introduces complex new zones (e.g., "Northern Surcharge"), the user can configure it entirely via the UI without developer intervention.
