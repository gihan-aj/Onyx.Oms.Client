using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Shared.Services;

namespace Onyx.Oms.Client.Desktop.Features.Products.Edit
{
    public partial class EditProductBaseLogisticsViewModel : ObservableObject
    {
        private readonly UpdateProductBaseLogisticsDto _originalBaseLogisticsDetails;
        private readonly UpdateDefaultVariantLogisticsDto? _originalDefaultVariantLogisticsDetails = null;
        private readonly ITenantProfileService _tenantProfileService;

        private decimal _baseCostAmount;
        public decimal BaseCostAmount
        {
            get => _baseCostAmount;
            set => SetProperty(ref _baseCostAmount, value);
        }
        private string _baseCostCurrency = string.Empty;
        public string BaseCostCurrency
        {
            get => _baseCostCurrency;
            set => SetProperty(ref _baseCostCurrency, value);
        }

        private decimal _basePriceAmount;
        public decimal BasePriceAmount
        {
            get => _basePriceAmount;
            set => SetProperty(ref _basePriceAmount, value);
        }
        private string _basePriceCurrency = string.Empty;
        public string BasePriceCurrency
        {
            get => _basePriceCurrency;
            set => SetProperty(ref _basePriceCurrency, value);
        }

        private decimal? _baseWeightAmount;
        public decimal? BaseWeightAmount
        {
            get => _baseWeightAmount;
            set
            {
                if(SetProperty(ref _baseWeightAmount, value))
                {
                    OnPropertyChanged(nameof(IsPhysycalProduct));
                }
            }
        }

        private bool _isPhysycalProduct;
        public bool IsPhysycalProduct
        {
            get => _isPhysycalProduct;
            set => SetProperty(ref _isPhysycalProduct, value);
        }

        private string? _baseWeightUnit;
        public string? BaseWeightUnit
        {
            get => _baseWeightUnit;
            set => SetProperty(ref _baseWeightUnit, value);
        }

        private bool _hasVariants;
        public bool HasVariants
        {
            get => _hasVariants;
            set
            {
                if (SetProperty(ref _hasVariants, value))
                {
                    OnPropertyChanged(nameof(ShowStockFields));
                }
            }
        }

        public bool ShowStockFields => !HasVariants;

        private int _stockOnHand;
        public int StockOnHand
        {
            get => _stockOnHand;
            set => SetProperty(ref _stockOnHand, value);
        }

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

        public EditProductBaseLogisticsViewModel(ProductDetailsDto product, ITenantProfileService tenantProfileService)
        {
            _tenantProfileService = tenantProfileService;
            _originalBaseLogisticsDetails = new UpdateProductBaseLogisticsDto
            {
                Id = product.Id,
                BaseCost = new MoneyDto { Amount = product.BaseCostAmount, Currency = product.BaseCostCurrency },
                BasePrice = new MoneyDto { Amount = product.BasePriceAmount, Currency = product.BaseCostCurrency },
                BaseWeight = new WeightDto { Value = product.BaseWeightAmount, Unit = product.BaseWeightUnit }
            };

            BaseCostAmount = product.BaseCostAmount;
            BaseCostCurrency = product.BaseCostCurrency;
            BasePriceAmount = product.BasePriceAmount;
            BasePriceCurrency = product.BasePriceCurrency;
            BaseWeightAmount = product.BaseWeightAmount;
            BaseWeightUnit = product.BaseWeightUnit;
            HasVariants = product.HasVariants;
            IsPhysycalProduct = BaseWeightAmount != null && BaseWeightAmount > 0;

            if (!HasVariants)
            {
                _originalDefaultVariantLogisticsDetails = new UpdateDefaultVariantLogisticsDto
                {
                    ProductId = product.Id,
                    Sku = product.BaseSku,
                    Cost = new MoneyDto { Amount = product.BaseCostAmount, Currency = product.BaseCostCurrency },
                    Price = new MoneyDto { Amount = product.BasePriceAmount, Currency = product.BasePriceCurrency },
                    Weight = new WeightDto { Value = product.BaseWeightAmount, Unit = product.BaseWeightUnit },
                    StockOnHand = product.StockOnHand
                };

                StockOnHand = product.StockOnHand;
            }

            BeginEditCommand = new RelayCommand(BeginEdit);
            CancelEditCommand = new RelayCommand(CancelEdit);
        }

        private void BeginEdit()
        {
            BaseCostCurrency = _tenantProfileService.Profile?.BaseCurrency ?? "LKR";
            BasePriceCurrency = _tenantProfileService.Profile?.BaseCurrency ?? "LKR";
            BaseWeightUnit = _tenantProfileService.Profile?.WeightUnit ?? "kg";

            IsEditing = true;
        }

        private void CancelEdit()
        {
            BaseCostAmount = _originalBaseLogisticsDetails.BaseCost.Amount;
            BaseCostCurrency = _originalBaseLogisticsDetails.BaseCost.Currency;
            BasePriceAmount = _originalBaseLogisticsDetails.BasePrice.Amount;
            BasePriceCurrency = _originalBaseLogisticsDetails.BasePrice.Currency;
            BaseWeightAmount = _originalBaseLogisticsDetails.BaseWeight?.Value;
            BaseWeightUnit = _originalBaseLogisticsDetails.BaseWeight?.Unit;
            if (!HasVariants && _originalDefaultVariantLogisticsDetails != null)
            {
                StockOnHand = _originalDefaultVariantLogisticsDetails.StockOnHand;
            }
            IsEditing = false;
        }

        public (UpdateDefaultVariantLogisticsDto? defaultVariantLogistics, UpdateProductBaseLogisticsDto baseLogistics) GetUpdateDtos()
        {
            var weightDto = IsPhysycalProduct
                ? new WeightDto { Value = BaseWeightAmount.HasValue? BaseWeightAmount.Value : 0, Unit = BaseWeightUnit ?? "kg" }
                : null;

            var baseLogistics = new UpdateProductBaseLogisticsDto
            {
                Id = _originalBaseLogisticsDetails.Id,
                BaseCost = new MoneyDto { Amount = BaseCostAmount, Currency = BaseCostCurrency },
                BasePrice = new MoneyDto { Amount = BasePriceAmount, Currency = BasePriceCurrency },
                BaseWeight = weightDto
            };
            UpdateDefaultVariantLogisticsDto? defaultVariantLogistics = null;
            if (!HasVariants)
            {
                defaultVariantLogistics = new UpdateDefaultVariantLogisticsDto
                {
                    ProductId = _originalBaseLogisticsDetails.Id,
                    Sku = _originalDefaultVariantLogisticsDetails?.Sku ?? string.Empty,
                    Cost = new MoneyDto { Amount = BaseCostAmount, Currency = BaseCostCurrency },
                    Price = new MoneyDto { Amount = BasePriceAmount, Currency = BasePriceCurrency },
                    Weight = weightDto,
                    StockOnHand = StockOnHand
                };
            }
            return (defaultVariantLogistics, baseLogistics);
        }
    }
}
