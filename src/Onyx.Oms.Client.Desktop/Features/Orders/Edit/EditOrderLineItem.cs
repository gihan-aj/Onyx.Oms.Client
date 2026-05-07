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

        private OrderStatus _orderCurrentStatus;    
        public OrderStatus OrderCurrentStatus
        {
            get => _orderCurrentStatus;
            set
            {
                if (SetProperty(ref _orderCurrentStatus, value))
                {
                    OnPropertyChanged(nameof(IsFulfillmentActive));
                    OnPropertyChanged(nameof(CanAllocateStock));
                    OnPropertyChanged(nameof(CanCreateTask));
                    OnPropertyChanged(nameof(IsHistoricalRecord));
                }
            }
        }

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
            set
            {
                if (SetProperty(ref _allocatedQuantity, value))
                {
                    OnPropertyChanged(nameof(AllocatedContextText));
                    OnPropertyChanged(nameof(ShowsTaskWarning));
                    OnPropertyChanged(nameof(ShowsProgressBar));
                }
            }
        }

        private int _pendingQuantity;
        public int PendingQuantity
        {
            get => _pendingQuantity;
            set
            {
                if (SetProperty(ref _pendingQuantity, value))
                {
                    OnPropertyChanged(nameof(CanAllocateStock));
                    OnPropertyChanged(nameof(CanCreateTask));
                }
            }
        }

        private int _incomingStock;
        public int IncomingStock
        {
            get => _incomingStock;
            set
            {
                if (SetProperty(ref _incomingStock, value))
                {
                    OnPropertyChanged(nameof(IncomingContextText));
                    OnPropertyChanged(nameof(HasIncomingStock));
                    OnPropertyChanged(nameof(StockContextText));
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

        public bool IsFulfillmentActive =>
            OrderCurrentStatus == OrderStatus.Confirmed ||
            OrderCurrentStatus == OrderStatus.Processing ||
            OrderCurrentStatus == OrderStatus.ReadyToPack;

        public bool IsHistoricalRecord =>
            OrderCurrentStatus >= OrderStatus.Shipped;

        public bool RequiresFulfillment => IsFulfillmentActive && PendingQuantity > 0;

        // Show "Allocate" if they need items AND the warehouse has them
        public bool CanAllocateStock => AvailableQuantity > 0;

        // Show "Create Task" if they need items BUT the warehouse is empty
        public bool CanCreateTask =>
            IsFulfillmentActive && PendingQuantity > 0 && AvailableQuantity == 0;

        public string StockContextText
        {
            get
            {
                var parts = new List<string>();

                if (AvailableQuantity > 0)
                    parts.Add($"📦 {AvailableQuantity} in stock");
                else
                    parts.Add("⚠️ Out of stock");

                if (IncomingStock > 0)
                    parts.Add($"⏳ {IncomingStock} incoming");

                return string.Join("  •  ", parts);
            }
        }

        public string AllocatedContextText => AllocatedQuantity > 0
            ? $"🔒 {AllocatedQuantity} already allocated"
            : "No items allocated yet";

        public string AvailableContextText => AvailableQuantity > 0
            ? $"📦 {AvailableQuantity} available in warehouse"
            : "⚠️ 0 available in warehouse";

        public string IncomingContextText => $"🚚 {IncomingStock} incoming from active tasks"; 

        public bool ShowsProgressBar => Quantity > AllocatedQuantity;

        public bool ShowsTaskWarning => Quantity > (AllocatedQuantity + AvailableQuantity);

        public bool HasIncomingStock => IncomingStock > 0 && ShowsTaskWarning;

        public IRelayCommand<EditOrderLineItem>? RemoveCommand { get; set; }
        public IAsyncRelayCommand<EditOrderLineItem>? AllocateStockCommand { get; set; }

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
            OnPropertyChanged(nameof(ShowsTaskWarning));
            OnPropertyChanged(nameof(ShowsProgressBar));
        }

        void OnAvailableQuantityChanged(int value)
        {
            OnPropertyChanged(nameof(QuantityProgressBrush));
            OnPropertyChanged(nameof(CanAllocateStock));
            OnPropertyChanged(nameof(CanCreateTask));
            OnPropertyChanged(nameof(ShowsTaskWarning));
            OnPropertyChanged(nameof(AvailableContextText));
            OnPropertyChanged(nameof(StockContextText));
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
