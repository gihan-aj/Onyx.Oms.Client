using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;

namespace Onyx.Oms.Client.Desktop.Features.Products.Edit
{
    public partial class EditProductVariantViewModel : ObservableObject
    {
        public Guid Id { get; init; }
        public Guid ProductId { get; init; }
        public List<ProductVariantAttributeDto> Attributes { get; init; } = new();
        public string DisplayAttributes { get; init; } = string.Empty;

        private string? _sku;
        public string? Sku
        {
            get => _sku;
            set => SetProperty(ref _sku, value);
        }

        private double _costAmount;
        public double CostAmount
        {
            get => _costAmount;
            set => SetProperty(ref _costAmount, value);
        }

        private string _costCurrency = string.Empty;
        public string CostCurrency
        {
            get => _costCurrency;
            set => SetProperty(ref _costCurrency, value);
        }

        private double _priceAmount;
        public double PriceAmount
        {
            get => _priceAmount;
            set => SetProperty(ref _priceAmount, value);
        }

        private string _priceCurrency = string.Empty;
        public string PriceCurrency
        {
            get => _priceCurrency;
            set => SetProperty(ref _priceCurrency, value);
        }

        private double? _weightAmount;
        public double? WeightAmount
        {
            get => _weightAmount;
            set => SetProperty(ref _weightAmount, value);
        }

        private string? _weightUnit;
        public string? WeightUnit
        {
            get => _weightUnit;
            set => SetProperty(ref _weightUnit, value);
        }

        private int _stockOnHand;
        public int StockOnHand
        {
            get => _stockOnHand;
            set => SetProperty(ref _stockOnHand, value);
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if(SetProperty(ref _isActive, value))
                {
                    OnPropertyChanged(nameof(IsInactive));
                }
            }
        }

        public bool IsInactive => !IsActive;

        private bool _isEditing;
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

        public IRelayCommand ToggleStatusCommand {  get; }

        public EditProductVariantViewModel()
        {
            ToggleStatusCommand = new RelayCommand(ToggleStatus);
        }

        private void ToggleStatus()
        {
            IsActive = !IsActive;
        }
    }
}
