using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Onyx.Oms.Client.Desktop.Features.Products;
using Onyx.Oms.Client.Desktop.Shared.Constants;
using Onyx.Oms.Client.Desktop.Shared.Services;
using Onyx.Oms.Client.Desktop.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Settings
{
    public partial class PaymentMethodsViewModel : PagedDataGridViewModelBase<PaymentMethodGridItem>
    {
        private readonly ISettingsApi _settingsApi;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<PaymentMethodsViewModel> _logger;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private string _selectedStatus = "Active";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                {
                    Page = 1;
                    LoadDataCommand.ExecuteAsync(null);
                }
            }
        }

        public ObservableCollection<string> StatusOptions { get; } = new(new[] { "Active", "Inactive", "All" });

        public bool CanEditPaymentMethods => _permissionService.CanExecute(Permissions.PaymentMethods.Edit);
        public bool CanActivatePaymentMethods => _permissionService.CanExecute(Permissions.PaymentMethods.Activate);
        public bool CanDeactivatePaymentMethods => _permissionService.CanExecute(Permissions.PaymentMethods.Deactivate);

        public PaymentMethodsViewModel(ISettingsApi settingsApi, IPermissionService permissionService)
        {
            _settingsApi = settingsApi;
            _permissionService = permissionService;
            _logger = App.Current.Services.GetRequiredService<ILogger<PaymentMethodsViewModel>>();
        }

        protected override async Task LoadDataAsync()
        {
            if (IsListLoading)
                return;

            try
            {
                bool? activeFilter = SelectedStatus switch
                {
                    "Active" => true,
                    "Inactive" => false,
                    _ => null,
                };

                var result = await _settingsApi.GetPaymentMethods(
                    page: Page,
                    pageSize: PageSize,
                    searchTerm: string.Empty,
                    sortColumn: SortColumn,
                    sortOrder: SortOrder,
                    isActive: activeFilter);

                Items.Clear();

                foreach (var item in result.Items)
                {
                    var gridItem = item.ToGridItem(CanEditPaymentMethods, CanActivatePaymentMethods, CanDeactivatePaymentMethods);
                    Items.Add(gridItem);
                }

                Page = result.Page;
                TotalCount = result.TotalCount;
                HasNextPage = result.HasNextPage;
                HasPreviousPage = result.HasPreviousPage;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment methods");
            }
            finally
            {
                IsListLoading = false;
            }
        }

        public async Task UpdatePaymentMethodAsync(Guid id, string displayName, decimal feeRate)
        {
            try
            {
                IsBusy = true;

                var dto = new UpdatePaymentMethodRquest(displayName, feeRate);
                await _settingsApi.UpdatePaymentMethod(id, dto);

                await LoadDataCommand.ExecuteAsync(null); // Refresh data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update payment method.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task ActivatePaymentMethodAsync(Guid id)
        {
            try
            {
                IsBusy = true;
                await _settingsApi.ActivatePaymentMethod(id);
                await LoadDataCommand.ExecuteAsync(null); // Refresh data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to activate payment method.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task DeactivatePaymentMethodAsync(Guid id)
        {
            try
            {
                IsBusy = true;
                await _settingsApi.DeactivatePaymentMethod(id);
                await LoadDataCommand.ExecuteAsync(null); // Refresh data
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deactivate payment method.");
            }
            finally
            {
                IsBusy = false;
            }
        }

    }
}
