using CommunityToolkit.Mvvm.ComponentModel;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.ProductPicker
{
    public partial class UiOptionValue : ObservableObject
    {
        public string Value { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public UiOptionValue(string value) => Value = value;
    }
}
