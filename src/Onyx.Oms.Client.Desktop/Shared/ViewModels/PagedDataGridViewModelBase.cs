using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.ViewModels
{
    public abstract partial class PagedDataGridViewModelBase<TItem> : ObservableObject
    {
        // --- Data ---
        private ObservableCollection<TItem> _items = new();

        public ObservableCollection<TItem> Items
        {
            get => _items;
            set
            {
                if(SetProperty(ref _items, value))
                {
                    OnPropertyChanged(nameof(HasNoData));
                }
            }
        }

        // -- Pagination --
        private int _page = 1;
        public int Page
        {
            get => _page;
            set
            {
                if(SetProperty(ref _page, value))
                {
                    OnPropertyChanged(nameof(PageSummary)); 
                }
            }
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                if(SetProperty(ref _pageSize, value))
                {
                    Page = 1;
                    LoadDataCommand.ExecuteAsync(null);
                }
            }
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set
            {
                if (SetProperty(ref _totalCount, value))
                {
                    OnPropertyChanged(nameof(PageSummary));
                }
            }
        }

        private bool _hasNextPage;
        public bool HasNextPage
        {
            get => _hasNextPage;
            set => SetProperty(ref _hasNextPage, value);
        }

        private bool _hasPreviousPage;
        public bool HasPreviousPage
        {
            get => _hasPreviousPage;
            set => SetProperty(ref _hasPreviousPage, value);
        }

        public ObservableCollection<int> PageSizes { get; } = new(new[] { 5, 10, 25, 50, 100 });

        // -- Sorting --
        protected string? SortColumn;
        protected string? SortOrder;

        // -- UI State --
        private bool _isListLoading;
        public bool IsListLoading
        {
            get => _isListLoading;
            set
            {
                if(SetProperty(ref _isListLoading, value))
                {
                    OnPropertyChanged(nameof(HasNoData));
                }
            }
        }

        public bool HasNoData => Items.Count == 0 && !IsListLoading;
        public string PageSummary => $"Page {Page} (Total: {TotalCount})";

        // --- Commands ---
        public IAsyncRelayCommand LoadDataCommand { get; }
        public IAsyncRelayCommand NextPageCommand { get; }
        public IAsyncRelayCommand PreviousPageCommand { get; }
        public IAsyncRelayCommand RefreshCommand { get; }

        public PagedDataGridViewModelBase()
        {
            LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
            NextPageCommand = new AsyncRelayCommand(NextPageAsync);
            PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        }

        // Abstract method to load data
        protected abstract Task LoadDataAsync();

        public virtual async Task SortByAsync(string column, string order)
        {
            SortColumn = column;
            SortOrder = order;
            Page = 1;
            await LoadDataAsync();
        }

        public virtual async Task PreviousPageAsync()
        {
            if (HasPreviousPage)
            {
                Page--;
                await LoadDataAsync();
            }
        }

        public virtual async Task NextPageAsync()
        {
            if (HasNextPage)
            {
                Page++;
                await LoadDataAsync();
            }
        }

        public virtual async Task RefreshAsync()
        {
            Page = 1;
            SortColumn = null;
            SortOrder = null;
            await OnRefreshFiltersAsync();
            await LoadDataAsync();
        }

        // Optional hook for derived classes to clear their specific filters during a refresh
        protected virtual Task OnRefreshFiltersAsync() => Task.CompletedTask;
    }
}
