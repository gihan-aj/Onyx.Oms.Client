using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products.Edit
{
    public partial class EditProductTagsViewModel : ObservableObject
    {
        private readonly UpdateProductBasicInfoDto _originalDetails;

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

        public ObservableCollection<string> Tags { get; } = new();

        public bool HasChanges =>
            Tags.Count != _originalDetails.Tags.Count ||
            Tags.Except(_originalDetails.Tags).Any();

        public IRelayCommand BeginEditCommand { get; }
        public IRelayCommand CancelEditCommand { get; }

        public EditProductTagsViewModel(ProductDetailsDto product)
        {
            _originalDetails = new UpdateProductBasicInfoDto
            {
                Id = product.Id,
                Name = product.Name,
                BaseSku = product.BaseSku,
                Description = product.Description,
                CategoryId = product.CategoryId,
                Tags = product.Tags,
            };

            if(product.Tags != null && product.Tags.Any())
            {
                foreach(var tag in product.Tags)
                    Tags.Add(tag);
            }

            BeginEditCommand = new RelayCommand(BeginEdit);
            CancelEditCommand = new RelayCommand(CancelEdit);
        }

        public async Task<UpdateProductBasicInfoDto> GetUpdateDto()
        {
            return new UpdateProductBasicInfoDto
            {
                Id = _originalDetails.Id,
                Name = _originalDetails.Name,
                BaseSku = _originalDetails.BaseSku,
                Description = _originalDetails.Description,
                CategoryId = _originalDetails.CategoryId,
                Tags = Tags.ToList()
            };
        }

        private void BeginEdit()
        {
            IsEditing = true;
        }

        private void CancelEdit()
        {
            if (HasChanges)
            {
                Tags.Clear();
                foreach (var tag in _originalDetails.Tags)
                {
                    Tags.Add(tag);
                }
            }
           
            IsEditing = false;
        }
    }
}
