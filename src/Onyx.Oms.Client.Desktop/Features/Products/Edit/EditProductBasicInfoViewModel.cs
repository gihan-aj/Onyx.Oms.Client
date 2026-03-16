using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products.Edit
{
    public partial class EditProductBasicInfoViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;
        private readonly UpdateProductBasicInfoDto _originalDetails;
        private readonly string _originalCategoryName;
        private readonly string _oroginalCategoryPath;
        private readonly Action<ProductCategoryDto> _onCategoryChangedCallback;

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _baseSku = string.Empty;
        public string BaseSku
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

        private ProductCategoryDto? _selectedCategory;
        public ProductCategoryDto? SelectedCategory
        {
            get => _selectedCategory;
            set 
            {
                if(SetProperty(ref _selectedCategory, value))
                {
                    if(value != null)
                        _onCategoryChangedCallback?.Invoke(value);
                }
            }
        }

        private bool _isEditing = false;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if(SetProperty(ref _isEditing, value))
                {
                    OnPropertyChanged(nameof(IsReadonly));
                }
            }
        }

        public bool IsReadonly => !IsEditing;

        public ObservableCollection<string> Tags { get; } = new();

        public bool HasChanges => 
            Name != _originalDetails.Name ||
            BaseSku != _originalDetails.BaseSku ||
            Description != _originalDetails.Description ||
            (SelectedCategory?.Id ?? Guid.Empty) != _originalDetails.CategoryId ||
            Tags.Count != _originalDetails.Tags.Count || 
            Tags.Except(_originalDetails.Tags).Any();

        public IRelayCommand BeginEditCommand { get; }
        public IRelayCommand CancelEditCommand { get; }

        public EditProductBasicInfoViewModel(
            ProductDetailsDto product, 
            Action<ProductCategoryDto> onCategoryChangedCallBack,
            IDialogService dialogService)
        {
            _dialogService = dialogService;
            _onCategoryChangedCallback = onCategoryChangedCallBack;
            _originalDetails = new UpdateProductBasicInfoDto
            {
                Id = product.Id,
                Name = product.Name,
                BaseSku = product.BaseSku,
                Description = product.Description,
                CategoryId = product.CategoryId,
                Tags = product.Tags,
            };
            _originalCategoryName = product.CategoryName;
            _oroginalCategoryPath = product.CategoryPath;

            Name = product.Name;
            BaseSku = product.BaseSku;
            Description = product.Description;
            SelectedCategory = new ProductCategoryDto
            {
                Id = product.CategoryId,
                Name = product.CategoryName,
                NamePath = product.CategoryPath
            };

            BeginEditCommand = new RelayCommand(BeginEdit);
            CancelEditCommand = new RelayCommand(CancelEdit);
        }

        private async Task<bool> IsValid()
        {
            var validationsErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(Name))
            {
                validationsErrors.Add("Name is required.");
            }
            if (string.IsNullOrWhiteSpace(BaseSku))
            {
                validationsErrors.Add("Base SKU is required.");
            }
            if (SelectedCategory == null || SelectedCategory.Id == Guid.Empty)
            {
                validationsErrors.Add("Category is required.");
            }

            if (validationsErrors.Count > 0)
                await _dialogService.ShowValidationErrorsAsync("Validation Failed", validationsErrors);

            return validationsErrors.Count == 0;
        }

        public async Task<UpdateProductBasicInfoDto?> GetUpdateDto()
        {
            if(await IsValid())
            {
                return new UpdateProductBasicInfoDto
                {
                    Id = _originalDetails.Id,
                    Name = Name,
                    BaseSku = BaseSku,
                    Description = Description,
                    CategoryId = SelectedCategory!.Id,
                    Tags = Tags.ToList()
                };
            }

            return null;
        }

        private void BeginEdit()
        {
            IsEditing = true;
        }

        private void CancelEdit()
        {
            if (HasChanges)
            {
                Name = _originalDetails.Name;
                BaseSku = _originalDetails.BaseSku;
                Description = _originalDetails.Description;
                SelectedCategory = new ProductCategoryDto
                {
                    Id = _originalDetails.CategoryId,
                    Name = _originalCategoryName,
                    NamePath = _oroginalCategoryPath
                };
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
