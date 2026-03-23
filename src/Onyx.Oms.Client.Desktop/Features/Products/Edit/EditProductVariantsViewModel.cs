using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Onyx.Oms.Client.Desktop.Features.Products.Edit
{
    public partial class EditProductVariantsViewModel : ObservableObject
    {
        private readonly string _currency = "LKR";
        private readonly string _weightUnit = "kg";
        private readonly List<ProductDetailsVariantDto> _originalVariants = new();

        private bool _hasVariants;
        public bool HasVariants
        {
            get => _hasVariants;
            set => SetProperty(ref _hasVariants, value);
        }

        public ObservableCollection<EditProductVariantViewModel> Variants { get; } = new();

        private bool _isEditing = false;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (SetProperty(ref _isEditing, value))
                {
                    OnPropertyChanged(nameof(IsReadonly));
                }
            }
        }

        public bool IsReadonly => !IsEditing;

        public IRelayCommand BeginEditCommand { get; }
        public IRelayCommand CancelEditCommand { get; }

        public EditProductVariantsViewModel(ProductDetailsDto product, ITenantProfileService tenantProfileService)
        {
            _currency = tenantProfileService.Profile?.BaseCurrency ?? "LKR";
            _weightUnit = tenantProfileService.Profile?.WeightUnit ?? "kg";
            HasVariants = product.HasVariants;

            if (HasVariants)
            {
                PopulateVariants(product.Variants);
            }

            BeginEditCommand = new RelayCommand(BeginEdit);
            CancelEditCommand = new RelayCommand(CancelEdit);
        }

        private void PopulateVariants(List<ProductDetailsVariantDto> variantDtos)
        {
            Variants.Clear();
            _originalVariants.Clear();

            foreach (var v in variantDtos)
            {
                _originalVariants.Add(v); // Store for cancel/revert

                // Create a nice display string (e.g., "Red - XL")
                var displayAttrs = string.Join(" - ", v.Attributes.Select(a => a.Value));

                Variants.Add(new EditProductVariantViewModel
                {
                    Id = v.Id,
                    ProductId = v.Id,
                    Attributes = v.Attributes,
                    DisplayAttributes = displayAttrs,
                    Sku = v.Sku,
                    CostAmount = (double)v.CostAmount,
                    CostCurrency = v.CostCurrency,
                    PriceAmount = (double)v.PriceAmount,
                    PriceCurrency = v.CostCurrency,
                    WeightAmount = v.WeightAmount.HasValue ? (double)v.WeightAmount : null,
                    WeightUnit = v.WeightUnit,
                    StockOnHand = v.StockOnHand
                });
            }
        }

        private void BeginEdit()
        {
            foreach(var v in Variants)
            {
                v.CostCurrency = _currency;
                v.PriceCurrency = _currency;
                v.WeightUnit = _weightUnit;
            }
            IsEditing = true;
            
        }

        private void CancelEdit()
        {
            Variants.Clear();
            foreach (var v in _originalVariants)
            {
                var displayAttrs = string.Join(" - ", v.Attributes.Select(a => a.Value));

                Variants.Add(new EditProductVariantViewModel
                {
                    Id = v.Id,
                    ProductId = v.Id,
                    Attributes = v.Attributes,
                    DisplayAttributes = displayAttrs,
                    Sku = v.Sku,
                    CostAmount = (double)v.CostAmount,
                    CostCurrency= v.CostCurrency,
                    PriceAmount = (double)v.PriceAmount,
                    PriceCurrency= v.PriceCurrency,
                    WeightAmount = v.WeightAmount.HasValue ? (double)v.WeightAmount : null,
                    WeightUnit = v.WeightUnit,
                    StockOnHand = v.StockOnHand
                });
            }
            IsEditing = false;
        }

        public List<UpdateProductVariantLogisticsDto> GetUpdateDtos(Guid productId)
        {
            var updates = new List<UpdateProductVariantLogisticsDto>();

            if (!HasVariants)
                return updates;

            foreach (var vm in Variants)
            {
                // Find the original to see if anything actually changed to save API calls
                var original = _originalVariants.FirstOrDefault(v => v.Id == vm.Id);

                if (original != null &&
                   (original.CostAmount != (decimal)vm.CostAmount ||
                    original.PriceAmount != (decimal)vm.PriceAmount ||
                    original.WeightAmount != (decimal)(vm.WeightAmount ?? 0) ||
                    original.StockOnHand != vm.StockOnHand))
                {
                    updates.Add(new UpdateProductVariantLogisticsDto
                    {
                        ProductId = productId,
                        VariantId = vm.Id,
                        Cost = new MoneyDto { Amount = (decimal)vm.CostAmount, Currency = vm.CostCurrency },
                        Price = new MoneyDto { Amount = (decimal)vm.PriceAmount, Currency = vm.PriceCurrency },
                        Weight = new WeightDto { Value = (decimal)(vm.WeightAmount ?? 0), Unit = vm.WeightUnit ?? "kg" },
                        StockOnHand = vm.StockOnHand
                    });
                }
            }

            return updates;
        }
    }
}
