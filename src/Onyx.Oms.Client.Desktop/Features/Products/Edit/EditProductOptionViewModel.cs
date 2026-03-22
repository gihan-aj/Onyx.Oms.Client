using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Onyx.Oms.Client.Desktop.Features.Products.Edit
{
    public partial class EditProductOptionViewModel : ObservableObject
    {
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref  _name, value);
        }

        private int _displayOrder;
        public int DisplayOrder
        {
            get => _displayOrder;
            set => SetProperty(ref _displayOrder, value);
        }

        public ObservableCollection<string> Values { get; } = new();
    }
}
