using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Diagnostics.Latency;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Onyx.Oms.Client.Desktop.Features.Products.Edit
{
    public partial class EditProductSpecificationsViewModel: ObservableObject
    {
        private readonly List<ProductSpecificationDto> _originalProductSpecs;
        private readonly IDialogService _dialogService;

        private bool _isEditing;
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

        public IRelayCommand BeginEditCommand { get; }
        public IRelayCommand CancelEditCommand { get; }

        public ObservableCollection<SpecFieldViewItem> SpecFields { get; } = new();

        public EditProductSpecificationsViewModel(
            List<SpecDefinition> specDefinitions, 
            List<ProductSpecificationDto> productSpecs, 
            IDialogService dialogService)
        {
            _dialogService = dialogService;
            _originalProductSpecs = productSpecs ?? new List<ProductSpecificationDto>();
            RebuildFields(specDefinitions);

            BeginEditCommand = new RelayCommand(BeginEdit);
            CancelEditCommand = new RelayCommand(CancelEdit);
        }

        public void RebuildFields(List<SpecDefinition> specDefinitions)
        {
            SpecFields.Clear();
            foreach (var spec in specDefinitions)
            {
                // Try to find if this product already has a value for this specification key
                var existingValue = _originalProductSpecs.FirstOrDefault(s => s.Key == spec.Key);

                var viewItem = new SpecFieldViewItem
                {
                    Key = spec.Key,
                    Label = spec.Label,
                    Type = spec.Type,
                    IsRequired = spec.IsRequired,
                    // If the product had a value, use it. Otherwise, start blank.
                    Value = existingValue != null ? existingValue.Value : string.Empty,
                };

                // If it's a dropdown/choice type, populate the options
                if (spec.Options != null)
                {
                    foreach (var opt in spec.Options)
                        viewItem.Options.Add(opt);
                }

                SpecFields.Add(viewItem);
            }
        }

        // Need to validate if all Required fields have values
        private async Task<bool> IsValid()
        {
            var validationErrors = new List<string>();

            foreach (var spec in SpecFields)
            {
                if (spec.IsRequired && string.IsNullOrWhiteSpace(spec.Value))
                {
                    validationErrors.Add($"The field '{spec.Label}' is required.");
                }
            }

            if (validationErrors.Any())
            {
                await _dialogService.ShowValidationErrorsAsync("Validation Errors",validationErrors);
            }

            return validationErrors.Count == 0;
        }

        public async Task<UpdateProductSpecificationsDto?> GetUpdateDto(Guid productId)
        {
            if (!await IsValid())
                return null;

            var dictionary = new Dictionary<string, string>();
            foreach (var field in SpecFields)
            {
                // Only send fields that actually have a value
                if (!string.IsNullOrWhiteSpace(field.Value))
                {
                    dictionary[field.Key] = field.Value;
                }
            }

            return new UpdateProductSpecificationsDto
            {
                Id = productId,
                Specifications = dictionary
            };
        }

        private void BeginEdit()
        {
            IsEditing = true;
        }

        private bool HasChanges()
        {
            // Has to check if any Value is changed for each Key between _originalProductSpecs and SpecFields
            foreach (var field in SpecFields)
            {
                var originalSpec = _originalProductSpecs.FirstOrDefault(s => s.Key == field.Key);
                var originalValue = originalSpec != null ? originalSpec.Value : string.Empty;
                if (field.Value != originalValue)
                {
                    return true;
                }
            }
            return false;
        }

        private void CancelEdit()
        {
            if (HasChanges())
            {
                // Has to restore values from _originalProductSpecs to SpecFields
                foreach (var field in SpecFields)
                {
                    var originalSpec = _originalProductSpecs.FirstOrDefault(s => s.Key == field.Key);
                    field.Value = originalSpec != null ? originalSpec.Value : string.Empty;
                }
            }

            IsEditing = false;
        }

        public void AcceptChanges()
        {
            _originalProductSpecs.Clear();

            foreach (var field in SpecFields)
            {
                if (!string.IsNullOrWhiteSpace(field.Value))
                {
                    _originalProductSpecs.Add(new ProductSpecificationDto
                    {
                        Key = field.Key,
                        Label = field.Label,
                        Value = field.Value
                    });
                }
            }

            IsEditing = false;
        }
    }
}
