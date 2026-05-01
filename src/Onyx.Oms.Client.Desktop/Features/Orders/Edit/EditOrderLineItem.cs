using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Orders.Edit
{
    public partial class EditOrderLineItem : ObservableObject
    {
        private readonly IFileService _fileService;

        public Guid? Id { get; set; }
        public OrderStatus OrderCurrentStatus { get; set; }

        private Guid _productId;
        public Guid ProductId
        {
            get => _productId;
            set => SetProperty(ref _productId, value);
        }

        public Guid ProductVariantId { get; set; }

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

        private OrderItemStatus _status;
        public OrderItemStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string BaseCurrency = "LKR";

        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (SetProperty(ref _unitPrice, value))
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
                if (SetProperty(ref _quantity, value))
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
                if (SetProperty(ref _availableQuantity, value))
                {
                    OnAvailableQuantityChanged(value);
                }
            }
        }

        private int _allocatedQuantity;
        public int AllocatedQuantity
        {
            get => _allocatedQuantity;
            set => SetProperty(ref _allocatedQuantity, value);
        }

        private int _pendingQuantity;
        public int PendingQuantity
        {
            get => _pendingQuantity;
            set => SetProperty(ref _pendingQuantity, value);
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

        public bool IsFulfillmentActive =>
            OrderCurrentStatus == OrderStatus.Confirmed ||
            OrderCurrentStatus == OrderStatus.Processing ||
            OrderCurrentStatus == OrderStatus.ReadyToPack;

        // Show "Allocate" if they need items AND the warehouse has them
        public bool CanAllocateStock =>
            IsFulfillmentActive && PendingQuantity > 0 && AvailableQuantity > 0;

        // Show "Create Task" if they need items BUT the warehouse is empty
        public bool CanCreateTask =>
            IsFulfillmentActive && PendingQuantity > 0 && AvailableQuantity == 0;

        public IRelayCommand<EditOrderLineItem>? RemoveCommand { get; set; }

        public EditOrderLineItem(IFileService fileService)
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
