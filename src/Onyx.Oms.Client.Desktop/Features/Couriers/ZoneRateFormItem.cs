using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

/// <summary>
/// Bindable view model representing a single editable zone rate row in the courier form.
/// </summary>
public partial class ZoneRateFormItem : ObservableObject
{
    /// <summary>
    /// Null for newly added rows; populated for rows loaded from the server.
    /// </summary>
    public Guid? Id { get; set; }

    private string _zoneName = string.Empty;
    public string ZoneName
    {
        get => _zoneName;
        set
        {
            if (SetProperty(ref _zoneName, value))
                OnPropertyChanged(nameof(DisplayName));
        }
    }

    private decimal _baseFee;
    public decimal BaseFee
    {
        get => _baseFee;
        set => SetProperty(ref _baseFee, value);
    }

    private decimal _baseWeight;
    public decimal BaseWeight
    {
        get => _baseWeight;
        set => SetProperty(ref _baseWeight, value);
    }

    private decimal _excessFeePerWeightUnit;
    public decimal ExcessFeePerWeightUnit
    {
        get => _excessFeePerWeightUnit;
        set => SetProperty(ref _excessFeePerWeightUnit, value);
    }

    private decimal _codPercentage;
    public decimal CodPercentage
    {
        get => _codPercentage;
        set => SetProperty(ref _codPercentage, value);
    }

    private string _currency = "LKR";
    public string Currency
    {
        get => _currency;
        set => SetProperty(ref _currency, value);
    }

    private string _weightUnit = "kg";
    public string WeightUnit
    {
        get => _weightUnit;
        set => SetProperty(ref _weightUnit, value);
    }

    private bool _isDefault;
    public bool IsDefault
    {
        get => _isDefault;
        set
        {
            if (SetProperty(ref _isDefault, value))
            {
                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(DistrictsSummary));
            }
        }
    }

    /// <summary>
    /// The districts this zone covers. Empty when IsDefault = true (matches all unmatched districts).
    /// </summary>
    public ObservableCollection<string> CoveredDistricts { get; set; } = new();

    // ── Computed display helpers ─────────────────────────────────────────

    /// <summary>Zone name with a "(Default)" suffix when applicable.</summary>
    public string DisplayName => IsDefault ? $"{ZoneName} (Default)" : ZoneName;

    /// <summary>Comma-separated district list shown in the table row, or a placeholder.</summary>
    public string DistrictsSummary
    {
        get
        {
            if (IsDefault) return "All unmatched districts";
            if (CoveredDistricts.Count == 0) return "—";
            return string.Join(", ", CoveredDistricts);
        }
    }

    /// <summary>Creates a shallow copy suitable for passing into the edit dialog.</summary>
    public ZoneRateFormItem Clone() => new()
    {
        Id = Id,
        ZoneName = ZoneName,
        BaseFee = BaseFee,
        BaseWeight = BaseWeight,
        ExcessFeePerWeightUnit = ExcessFeePerWeightUnit,
        CodPercentage = CodPercentage,
        Currency = Currency,
        WeightUnit = WeightUnit,
        IsDefault = IsDefault,
        CoveredDistricts = new ObservableCollection<string>(CoveredDistricts)
    };

    /// <summary>Copies all values from <paramref name="source"/> into this item (in-place update).</summary>
    public void CopyFrom(ZoneRateFormItem source)
    {
        ZoneName = source.ZoneName;
        BaseFee = source.BaseFee;
        BaseWeight = source.BaseWeight;
        ExcessFeePerWeightUnit = source.ExcessFeePerWeightUnit;
        CodPercentage = source.CodPercentage;
        Currency = source.Currency;
        WeightUnit = source.WeightUnit;
        IsDefault = source.IsDefault;
        CoveredDistricts = new ObservableCollection<string>(source.CoveredDistricts);
        OnPropertyChanged(nameof(DistrictsSummary));
        OnPropertyChanged(nameof(DisplayName));
    }
}
