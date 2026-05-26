using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Onyx.Oms.Client.Desktop.Shared.Constants;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Onyx.Oms.Client.Desktop.Features.Couriers;

/// <summary>
/// Helper bindable item used by the district checkbox GridView.
/// </summary>
public sealed class DistrictCheckItem
{
    public string Name { get; set; } = string.Empty;

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => _isSelected = value;
    }
}

/// <summary>
/// ContentDialog for adding or editing a single <see cref="ZoneRateFormItem"/>.
/// Pass a cloned item for edit, or a new item for add.
/// After <see cref="ContentDialogResult.Primary"/>, read <see cref="Item"/> for the result.
/// </summary>
public sealed partial class ZoneRateDialog : ContentDialog
{
    /// <summary>The zone rate being edited. Mutated in place on Save.</summary>
    public ZoneRateFormItem Item { get; }

    /// <summary>Observable list of district check-items bound to the GridView.</summary>
    public ObservableCollection<DistrictCheckItem> DistrictItems { get; } = new();

    private readonly string _originalTitle;

    public ZoneRateDialog(ZoneRateFormItem item, bool isEdit = false)
    {
        Item = item;
        _originalTitle = isEdit ? $"Edit Zone — {item.ZoneName}" : "Add Zone Rate";
        Title = _originalTitle;

        InitializeComponent();

        BuildDistrictItems();

        PrimaryButtonClick += OnPrimaryButtonClick;
    }

    // ── Initialisation ────────────────────────────────────────────────────

    private void BuildDistrictItems()
    {
        DistrictItems.Clear();
        foreach (var district in SriLankaDistricts.All)
        {
            DistrictItems.Add(new DistrictCheckItem
            {
                Name = district,
                IsSelected = Item.CoveredDistricts.Contains(district, StringComparer.OrdinalIgnoreCase)
            });
        }
    }

    // ── Event handlers ────────────────────────────────────────────────────

    private void OnIsDefaultToggled(object sender, RoutedEventArgs e)
    {
        // When switching to IsDefault = true, clear district selection
        // (a default zone doesn't need explicit mappings)
        if (Item.IsDefault)
        {
            foreach (var d in DistrictItems)
                d.IsSelected = false;
        }
    }

    private void OnDistrictChecked(object sender, RoutedEventArgs e)
    {
        if ((sender as CheckBox)?.DataContext is DistrictCheckItem item
            && !Item.CoveredDistricts.Contains(item.Name, StringComparer.OrdinalIgnoreCase))
        {
            Item.CoveredDistricts.Add(item.Name);
        }
        HideDistrictError();
    }

    private void OnDistrictUnchecked(object sender, RoutedEventArgs e)
    {
        if ((sender as CheckBox)?.DataContext is DistrictCheckItem item)
        {
            var existing = Item.CoveredDistricts
                .FirstOrDefault(d => string.Equals(d, item.Name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                Item.CoveredDistricts.Remove(existing);
        }
    }

    private void OnClearDistrictsClick(object sender, RoutedEventArgs e)
    {
        foreach (var d in DistrictItems)
            d.IsSelected = false;
        Item.CoveredDistricts.Clear();
    }

    // ── Validation & Primary button override ─────────────────────────────

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var valid = Validate();
        if (!valid)
        {
            // Defer close — stay open so user can correct errors
            args.Cancel = true;
        }
        else
        {
            // Sync selected districts back to the item in case the
            // DistrictCheckItem.IsSelected was toggled without firing events
            SyncDistrictsToItem();
        }
    }

    private bool Validate()
    {
        var isValid = true;

        // Zone name
        if (string.IsNullOrWhiteSpace(Item.ZoneName))
        {
            ZoneNameError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            ZoneNameError.Visibility = Visibility.Collapsed;
        }

        // At least one district required if not default
        if (!Item.IsDefault && Item.CoveredDistricts.Count == 0)
        {
            DistrictError.Visibility = Visibility.Visible;
            isValid = false;
        }
        else
        {
            DistrictError.Visibility = Visibility.Collapsed;
        }

        return isValid;
    }

    private void HideDistrictError() => DistrictError.Visibility = Visibility.Collapsed;

    /// <summary>
    /// Final sync: rebuild CoveredDistricts from the DistrictItems state.
    /// This ensures consistency regardless of how check events fired.
    /// </summary>
    private void SyncDistrictsToItem()
    {
        Item.CoveredDistricts.Clear();
        foreach (var d in DistrictItems.Where(d => d.IsSelected))
            Item.CoveredDistricts.Add(d.Name);
    }
}
