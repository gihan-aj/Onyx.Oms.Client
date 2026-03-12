using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Onyx.Oms.Client.Desktop.Features.Products.Details
{
    public partial class ProductOptionViewItem : ObservableObject
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<ProductOptionValueViewItem> Values { get; } = new();
    }

    public partial class ProductOptionValueViewItem : ObservableObject
    {
        private string _value = string.Empty;
        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private string _optionName = string.Empty;
        public string OptionName
        {
            get => _optionName;
            set => SetProperty(ref _optionName, value);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
