using CommunityToolkit.Mvvm.ComponentModel;

namespace Onyx.Oms.Client.Desktop.Features.Products
{
    public partial class SelectableTagViewModel : ObservableObject
    {
        public ImageOptionTag Tag { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                SetProperty(ref _isSelected, value);
            }
        }

        public SelectableTagViewModel(ImageOptionTag tag)
        {
            Tag = tag;
        }
    }
}
