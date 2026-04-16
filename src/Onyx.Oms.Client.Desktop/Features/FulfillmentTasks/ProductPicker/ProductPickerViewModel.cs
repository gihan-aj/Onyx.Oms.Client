using CommunityToolkit.Mvvm.Input;
using Onyx.Oms.Client.Desktop.Shared.Models;
using Onyx.Oms.Client.Desktop.Shared.Services;
using Onyx.Oms.Client.Desktop.Shared.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.FulfillmentTasks.ProductPicker
{
    public partial class ProductPickerViewModel : PagedDataGridViewModelBase<ProductPickerGridItem>
    {
        private readonly IFulfillmentTasksApi _api;
        private readonly IFileService _fileService;

        // -- Filtering --
        private string _searchTerm = string.Empty;
        public string SearchTerm
        {
            get => _searchTerm;
            set
            {
                if (SetProperty(ref _searchTerm, value))
                {
                    Page = 1;
                    LoadDataCommand.ExecuteAsync(null);
                }
            }
        }

        private ProductCategoryDto? _selectedProductCategory;
        public ProductCategoryDto? SelectedProductCategory
        {
            get => _selectedProductCategory;
            set
            {
                if (SetProperty(ref _selectedProductCategory, value))
                {
                    Page = 1;
                    LoadDataCommand.ExecuteAsync(null);
                }
            }
        }

        private ProductPickerGridItem? _selectedItem;
        public ProductPickerGridItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                // Unsubscribe from previous item
                if (_selectedItem != null)
                {
                    _selectedItem.PropertyChanged -= SelectedItem_PropertyChanged;
                }
                if(SetProperty(ref _selectedItem, value))
                {
                    // Subscribe to new item
                    if (_selectedItem != null)
                    {
                        _selectedItem.PropertyChanged += SelectedItem_PropertyChanged;
                    }
                    OnPropertyChanged(nameof(HasSelection));
                }
            }
        }

        private void SelectedItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProductPickerGridItem.ResolvedVariant))
            {
                // When a chip is clicked, re-evaluate if the Primary button should be enabled
                OnPropertyChanged(nameof(HasSelection));
            }
        }

        public bool HasSelection => SelectedItem != null && SelectedItem.ResolvedVariant != null;

        // -- Commands --
        public IAsyncRelayCommand ClearFiltersCommand { get; }

        public ProductPickerViewModel(
            IFulfillmentTasksApi api, 
            IFileService fileService)
        {
            _api = api;
            _fileService = fileService;

            ClearFiltersCommand = new AsyncRelayCommand(ClearFlitersAsync);
        }

        protected override async Task LoadDataAsync()
        {
            if (IsListLoading)
                return;
            try
            {
                IsListLoading = true;

                var result = await _api.GetProductsPaged(
                page: Page,
                pageSize: PageSize,
                searchTerm: string.IsNullOrWhiteSpace(SearchTerm) ? null : SearchTerm,
                sortColumn: SortColumn,
                sortOrder: SortOrder,
                isActive: true,
                categoryId: SelectedProductCategory?.Id,
                includeVariantsAndImages: true);

                Items.Clear();

                foreach (var item in result.Items)
                {
                    var gridItem = await item.ToPickerGridItem(_fileService);
                    gridItem.OnOptionInteraction = (interactedItem) =>
                    {
                        if (SelectedItem != interactedItem)
                            SelectedItem = interactedItem;
                    };
                    Items.Add(gridItem);
                }

                Page = result.Page;
                TotalCount = result.TotalCount;
                HasNextPage = result.HasNextPage;
                HasPreviousPage = result.HasPreviousPage;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading products: {ex.Message}");
            }
            finally
            {
                IsListLoading = false;
            }
        }

        protected override Task OnRefreshFiltersAsync()
        {
            SearchTerm = string.Empty;
            SelectedProductCategory = null;
            return Task.CompletedTask;
        }

        private async Task ClearFlitersAsync()
        {
            SearchTerm = string.Empty;
            SelectedProductCategory = null;
            Page = 1;
            await LoadDataAsync();
        }

        public async Task<PagedResult<ProductCategoryDto>> FetchCategoriesAsync(string searchTerm, int page, int pageSize, bool isLeafOnly, CancellationToken token = default)
        {
            try
            {
                return await _api.SearchCategories(
                    page: page,
                    pageSize: pageSize,
                    searchTerm: string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                    isActive: true,
                    isLeafOnly: isLeafOnly);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching categories: {ex.Message}");
                return new PagedResult<ProductCategoryDto> { Items = new(), Page = page, PageSize = pageSize, TotalCount = 0 };
            }
        }
    }
}
