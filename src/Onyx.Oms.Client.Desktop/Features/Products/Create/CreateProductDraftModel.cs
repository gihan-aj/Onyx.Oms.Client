using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products.Create
{
    public partial class CreateProductDraftModel : ObservableObject
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string? _baseSku;
        public string? BaseSku
        {
            get => _baseSku;
            set => SetProperty(ref _baseSku, value);
        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private Guid _categoryId;
        public Guid CategoryId
        {
            get => _categoryId;
            set => SetProperty(ref _categoryId, value);
        }

        private decimal _baseCostAmount;
        public decimal BaseCostAmount
        {
            get => _baseCostAmount;
            set => SetProperty(ref _baseCostAmount, value);
        }

        private decimal _basePriceAmount;
        public decimal BasePriceAmount
        {
            get => _basePriceAmount;
            set => SetProperty(ref _basePriceAmount, value);
        }

        private decimal? _baseWeightAmount;
        public decimal? BaseWeightAmount
        {
            get => _baseWeightAmount;
            set => SetProperty(ref _baseWeightAmount, value);
        }

        //private bool _hasVariants;
        //public bool HasVariants
        //{
        //    get => _hasVariants;
        //    set => SetProperty(ref _hasVariants, value);
        //}

        private int _baseStockOnHand;
        public int BaseStockOnHand
        {
            get => _baseStockOnHand;
            set => SetProperty(ref _baseStockOnHand, value);
        }

    }

    public partial class CreateProductOptionDraftModel : ObservableObject
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<string> Values { get; } = new();
    }

    public partial class CreateProductVariantDraftModel : ObservableObject
    {
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

        private double _priceAmount;
        public double PriceAmount
        {
            get => _priceAmount;
            set => SetProperty(ref _priceAmount, value);
        }

        private double? _weightValue;
        public double? WeightValue
        {
            get => _weightValue;
            set => SetProperty(ref _weightValue, value);
        }

        private int _stockOnHand;
        public int StockOnHand
        {
            get => _stockOnHand;
            set => SetProperty(ref _stockOnHand, value);
        }
    }

    public partial class CreateProductImageDraftModel : ObservableObject
    {
        public string FileName { get; } = string.Empty;

        // In-memory image
        public BitmapImage ImageSource { get; } = null!;

        public bool _isMain;
        public bool IsMain
        {
            get => _isMain;
            set => SetProperty(ref _isMain, value);
        }

        private int _displayOrder;
        public int DisplayOrder
        {
            get => _displayOrder;
            set => SetProperty(ref _displayOrder, value);
        }

        public ObservableCollection<SelectableTagViewItem> AvailableTags = new();

        public ImageOptionTag? SelectedTag => AvailableTags.FirstOrDefault(t => t.IsSelected)?.Tag;

        public CreateProductImageDraftModel(string fileName, BitmapImage imageSource, bool isMain)
        {
            FileName = fileName;
            ImageSource = imageSource;
            IsMain = isMain;
        }

        public static async Task<CreateProductImageDraftModel> CreateAsync(string fileName, byte[] imageBytes, bool isMain)
        {
            using var stream = new MemoryStream(imageBytes);
            using var randomAccessStream = stream.AsRandomAccessStream();

            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(randomAccessStream);

            return new CreateProductImageDraftModel(fileName, bitmapImage, isMain);
        }

        public void SyncTags(IEnumerable<ImageOptionTag> globalTags)
        {
            var previouslySelected = SelectedTag;
            AvailableTags.Clear();

            foreach (var tag in globalTags)
            {
                var tagViewItem = new SelectableTagViewItem(tag);

                // Restore the prev selection if the option wasn't deleted
                if(previouslySelected != null && tag.OptionName == previouslySelected.OptionName && tag.OptionValue == previouslySelected.OptionValue)
                {
                    tagViewItem.IsSelected = true;
                }

                // Enforce single selection
                tagViewItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(SelectableTagViewItem.IsSelected) && tagViewItem.IsSelected)
                    {
                        // Uncheck all other tags
                        foreach (var other in AvailableTags.Where(x => x != tagViewItem))
                        {
                            other.IsSelected = false;
                        }
                    }
                };

                AvailableTags.Add(tagViewItem);
            }
        }
    }

    public record ImageOptionTag(string OptionName, string OptionValue)
    {
        public string DisplayLabel => $"{OptionName}: {OptionValue}";
    }

    public partial class SelectableTagViewItem : ObservableObject
    {
        public ImageOptionTag Tag { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public SelectableTagViewItem(ImageOptionTag tag) => Tag = tag;
    }
}
