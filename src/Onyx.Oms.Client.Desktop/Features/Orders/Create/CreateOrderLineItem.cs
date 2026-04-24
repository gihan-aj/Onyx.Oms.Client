using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Create
{
    public partial class CreateOrderLineItem : ObservableObject
    {
        private readonly IFileService _fileService;

        private Guid _productId;
        public Guid ProductId 
        { 
            get => _productId;
            set => SetProperty(ref _productId, value);
        }

        public Guid? ProductVariantId { get; set; }

        private string _productName = string.Empty;
        public string ProductName
        {
            get => _productName;
            set => SetProperty(ref _productName, value);
        }

        private string _sku = string.Empty;
        public string Sku
        {
            get => _sku;
            set => SetProperty(ref _sku, value);
        }

        private string? _imageUrl;
        public string? ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public string BaseCurrency = "LKR";

        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if(SetProperty(ref _unitPrice, value))
                {
                    OnUnitPriceChanged(value);
                }
            }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if(SetProperty(ref _quantity, value))
                {
                    OnQuantityChanged(value);
                }
            }
        }

        private int _availableQuantity;
        public int AvailableQuantity
        {
            get => _availableQuantity;
            set
            {
                if(SetProperty(ref _availableQuantity, value))
                {
                    OnAvailableQuantityChanged(value);
                }
            }
        }

        private decimal _lineTotal;
        public decimal LineTotal
        {
            get => _lineTotal;
            set => SetProperty(ref _lineTotal, value);
        }

        private BitmapImage? _imageSource;
        public BitmapImage? ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        public CreateOrderLineItem(IFileService fileService)
        {
            _fileService = fileService;
        }

        public Microsoft.UI.Xaml.Media.Brush QuantityProgressBrush
        {
            get
            {
                if (AvailableQuantity >= Quantity) return (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["SystemFillColorSuccessBrush"];
                if (AvailableQuantity > 0) return (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["SystemFillColorCautionBrush"];
                return (Microsoft.UI.Xaml.Media.Brush)Microsoft.UI.Xaml.Application.Current.Resources["SystemFillColorCriticalBrush"];
            }
        }

        void OnQuantityChanged(int value)
        {
            LineTotal = Quantity * UnitPrice;
            OnPropertyChanged(nameof(QuantityProgressBrush));
        }

        void OnAvailableQuantityChanged(int value)
        {
            OnPropertyChanged(nameof(QuantityProgressBrush));
        }

        void OnUnitPriceChanged(decimal value)
        {
            LineTotal = Quantity * UnitPrice;
        }

        public async Task LoadImageAsync()
        {
            if (!string.IsNullOrEmpty(ImageUrl))
            {
                try
                {
                    var imageBytes = await _fileService.ReadFileAsync("ProductImages", ImageUrl);
                    if (imageBytes != null)
                    {
                        using var stream = new MemoryStream(imageBytes);
                        using var randomAccessStream = stream.AsRandomAccessStream();
                        var bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(randomAccessStream);

                        ImageSource = bitmap;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating image for product {ProductId}: {ex.Message}");
                }
            }
            else
            {
                ImageSource = null; // or set to a default image
            }
        }
    }
}
