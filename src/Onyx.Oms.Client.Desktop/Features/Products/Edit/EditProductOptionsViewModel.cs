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
    public partial class EditProductOptionsViewModel : ObservableObject
    {
        private readonly Guid _productId;
        private readonly List<ProductDetailsOptionDto> _originalOptions = new();
        private readonly IToastService _toastService;
        private readonly IDialogService _dialogService;

        private bool _hasVariants;
        public bool HasVariants
        {
            get => _hasVariants;
            set => SetProperty(ref _hasVariants, value);
        }

        public ObservableCollection<EditProductOptionViewModel> Options { get; } = new();

        private string _draftOptionName = string.Empty;
        public string DraftOptionName
        {
            get => _draftOptionName;
            set => SetProperty(ref _draftOptionName, value);
        }
        public ObservableCollection<string> DraftOptionValues { get; } = new();

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

        public bool HasChanges
        {
            get
            {
                if (HasVariants != _originalOptions.Any())
                    return true;
                if (HasVariants)
                {
                    if (Options.Count != _originalOptions.Count)
                        return true;
                    for (int i = 0; i < Options.Count; i++)
                    {
                        var optionVm = Options[i];
                        var originalOption = _originalOptions[i];
                        if (optionVm.Name != originalOption.Name ||
                            optionVm.DisplayOrder != originalOption.DisplayOrder ||
                            !optionVm.Values.SequenceEqual(originalOption.Values))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public IRelayCommand BeginEditCommand { get; }
        public IRelayCommand CancelEditCommand { get; }
        public IRelayCommand AddOptionCommand { get; }
        public IRelayCommand RemoveOptionCommand { get; }

        public EditProductOptionsViewModel(ProductDetailsDto product, IToastService toastService, IDialogService dialogService)
        {
            _productId = product.Id;
            HasVariants = product.HasVariants;
            _toastService = toastService;
            _dialogService = dialogService;

            if (HasVariants)
            {
                foreach (var option in product.Options)
                {
                    _originalOptions.Add(option);
                    var optionVm = new EditProductOptionViewModel
                    {
                        Name = option.Name,
                        DisplayOrder = option.DisplayOrder,
                    };
                    foreach (var val in option.Values)
                    {
                        optionVm.Values.Add(val);
                    }

                    Options.Add(optionVm);
                }
            }

            BeginEditCommand = new RelayCommand(BeginEdit);
            CancelEditCommand = new RelayCommand(CancelEdit);
            AddOptionCommand = new RelayCommand(AddOption);
            RemoveOptionCommand = new RelayCommand<EditProductOptionViewModel>(RemoveOption);
        }

        public void BeginEdit()
        {
            IsEditing = true;
        }

        public void CancelEdit()
        {
            IsEditing = false;

            Options.Clear();
            foreach (var option in _originalOptions)
            {
                var optionVm = new EditProductOptionViewModel
                {
                    Name = option.Name,
                    DisplayOrder = option.DisplayOrder,
                };
                foreach (var val in option.Values)
                {
                    optionVm.Values.Add(val);
                }
                Options.Add(optionVm);
            }
        
        }

        private void AddOption()
        {
            if (string.IsNullOrWhiteSpace(DraftOptionName) || !DraftOptionValues.Any())
            {
                _toastService.ShowWarning("Incomplete Info", "Please provide both an Option Name and at least one Value.");
                return;
            }

            var valuesList = DraftOptionValues
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();

            if (!valuesList.Any())
            {
                _toastService.ShowWarning("Incomplete Info", "Please provide valid values.");
                return;
            }

            if (Options.Any(o => o.Name.Equals(DraftOptionName, StringComparison.OrdinalIgnoreCase)))
            {
                _toastService.ShowWarning("Duplicate Option", "An option with this name already exists.");
                return;
            }

            if (Options.Count() >= 3)
            {
                _toastService.ShowWarning("Maximum Options Reached", "You can only have up to 3 option axes for a product.");
                return;
            }

            var optionModel = new EditProductOptionViewModel { Name = DraftOptionName };
            foreach (var v in valuesList)
            {
                optionModel.Values.Add(v);
            }

            Options.Add(optionModel);

            DraftOptionName = string.Empty;
            DraftOptionValues.Clear();
        }

        private void RemoveOption(EditProductOptionViewModel? option)
        {
            if (option != null)
            {
                Options.Remove(option);
            }
        }

        public async Task<UpdateProductOptionsDto?> GetUpdateDto()
        {
            if (!HasVariants)
                return null;

            bool axesChanged = false;
            bool valuesChanged = false;

            if (Options.Count != _originalOptions.Count)
            {
                axesChanged = true; // Added or removed a whole Option
            }
            else
            {
                for (int i = 0; i < Options.Count; i++)
                {
                    var current = Options[i];
                    var original = _originalOptions[i];

                    // Did they rename the Option (e.g., "Color" -> "Colors")? 
                    // Treat this as an axis change because it fundamentally alters the matrix.
                    if (!current.Name.Equals(original.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        axesChanged = true;
                        break; // No need to check further, we hit the severe warning
                    }

                    // If the axes are the same, did they change the tags inside?
                    if (!current.Values.SequenceEqual(original.Values))
                    {
                        valuesChanged = true;
                    }
                }
            }

            // 2. Show the appropriate warning
            if (axesChanged)
            {
                bool confirmed = await _dialogService.ShowConfirmationAsync(
                    "Regenerate All Variants?",
                    "Changing the main Option Categories (like adding or removing 'Size' or 'Color') will permanently delete ALL existing variants and generate a completely new set. Any custom pricing or stock on existing variants will be lost.\n\nAre you sure you want to proceed?",
                    "Yes, regenerate variants",
                    "Cancel");

                if (!confirmed) return null; // Abort save
            }
            else if (valuesChanged)
            {
                bool confirmed = await _dialogService.ShowConfirmationAsync(
                    "Update Variant Values?",
                    "Changing option values will delete variants tied to removed values, and generate missing variants for new values. Existing untouched variants will remain safe.\n\nDo you want to proceed?",
                    "Yes, update variants",
                    "Cancel");

                if (!confirmed) return null; // Abort save
            }

            // 3. If they confirmed (or if there were no changes), build the DTO
            var updatedOptions = new List<ProductOptionDto>();
            for (int i = 0; i < Options.Count; i++)
            {
                var optionVm = Options[i];
                var updatedOption = new ProductOptionDto
                {
                    Name = optionVm.Name,
                    Values = optionVm.Values.ToList()
                };
                updatedOptions.Add(updatedOption);
            }

            return new UpdateProductOptionsDto { Id = _productId, Options = updatedOptions };
        }
    }
}
