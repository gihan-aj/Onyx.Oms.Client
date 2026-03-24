using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Onyx.Oms.Client.Desktop.Shared.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Onyx.Oms.Client.Desktop.Features.Products.Edit
{
    public partial class EditProductImagesViewModel : ObservableObject
    {
        private readonly Guid _productId;
        private readonly IFileService _fileService;
        private readonly IToastService _toastService;

        private readonly List<ProductDetailsImageDto> _originalImages = new();

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

        public ObservableCollection<EditProductImageViewModel> Images { get; } = new();
        public ObservableCollection<EditProductImageOptionTag> AvailableTags { get; } = new();

        public IRelayCommand BeginEditCommand { get; }
        public IRelayCommand CancelEditCommand { get; }
        public IAsyncRelayCommand UploadImageCommand { get; }
        public IRelayCommand<EditProductImageViewModel?> SetMainImageCommand { get; }
        public IAsyncRelayCommand<EditProductImageViewModel?> DeleteImageCommand { get; }

        public EditProductImagesViewModel(ProductDetailsDto productDetails, IFileService fileService, IToastService toastService)
        {
            _productId = productDetails.Id;
            _fileService = fileService;
            _toastService = toastService;

            BeginEditCommand = new RelayCommand(BeginEdit);
            CancelEditCommand = new AsyncRelayCommand(CancelEditAsync);
            UploadImageCommand = new AsyncRelayCommand(UploadImageAsync);
            SetMainImageCommand = new RelayCommand<EditProductImageViewModel?>(SetMainImage);
            DeleteImageCommand = new AsyncRelayCommand<EditProductImageViewModel?>(DeleteImageAsync);
        }

        public async Task InitializeAsync(List<ProductDetailsImageDto> existingImages)
        {
            Images.Clear();
            _originalImages.Clear();

            foreach (var img in existingImages)
            {
                _originalImages.Add(img);

                // Fetch physical bytes from local storage
                var imageBytes = await _fileService.ReadFileAsync("ProductImages", img.Url);
                if (imageBytes != null)
                {
                    var previewImage = await EditProductImageViewModel.CreateAsync(img, imageBytes);
                    previewImage.SyncTags(AvailableTags, img.OptionName, img.OptionValue);
                    Images.Add(previewImage);
                }
            }
        }

        private void BeginEdit() => IsEditing = true;

        private async Task CancelEditAsync()
        {
            // GARBAGE COLLECTION: Delete any NEW files uploaded during this aborted edit session
            var newlyUploadedImages = Images.Where(i => i.Id == Guid.Empty).ToList();
            foreach (var img in newlyUploadedImages)
            {
                await _fileService.DeleteFileAsync("ProductImages", img.FileName);
            }

            // REVERT UI: Reload the original images
            Images.Clear();
            foreach(var img in _originalImages)
            {
                var imageBytes = await _fileService.ReadFileAsync("ProductImages", img.Url);
                if (imageBytes != null)
                {
                    var previewImage = await EditProductImageViewModel.CreateAsync(img, imageBytes);
                    previewImage.SyncTags(AvailableTags, img.OptionName, img.OptionValue);
                    Images.Add(previewImage);
                }
            }

            IsEditing = false;
        }

        // Called by the Main ViewModel ONLY IF the API update is successful!
        public async Task AcceptChangesAsync()
        {
            // GARBAGE COLLECTION: Physically delete files the user removed from the UI
            var deletedImages = _originalImages.Where(orig => !Images.Any(i => i.Id == orig.Id)).ToList();
            foreach (var img in deletedImages)
            {
                await _fileService.DeleteFileAsync("ProductImages", img.Url);
            }

            // Lock in the new state as the "Original" state
            _originalImages.Clear();
            foreach (var img in Images)
            {
                _originalImages.Add(new ProductDetailsImageDto
                {
                    Id = img.Id == Guid.Empty ? Guid.NewGuid() : img.Id, // Simulate DB ID assignment
                    Url = img.FileName,
                    DisplayOrder = img.DisplayOrder,
                    IsMain = img.IsMain,
                    OptionName = img.SelectedTag?.OptionName,
                    OptionValue = img.SelectedTag?.OptionValue
                });
            }

            IsEditing = false;
        }

        public void RefreshAvailableImageTags(List<ProductDetailsOptionDto> options)
        {
            var currentSelections = Images.Select(i => i.SelectedTag).ToList();

            AvailableTags.Clear();

            foreach (var option in options)
            {
                foreach (var val in option.Values)
                {
                    AvailableTags.Add(new EditProductImageOptionTag(option.Name, val));
                }
            }

            foreach (var img in Images)
            {
                img.SyncTags(AvailableTags);
            }
        }

        private async Task UploadImageAsync()
        {
            try
            {
                var picker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.Thumbnail,
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary
                };

                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");

                // WinUI 3 requirement: Bind the picker to the current Window HWND
                var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
                InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file == null)
                    return;

                // Unique name
                string extension = Path.GetExtension(file.Path);
                string newFileName = $"{Guid.NewGuid()}{extension}";

                using var stream = await file.OpenStreamForReadAsync();

                // resize, write
                await _fileService.SaveImageAsync("ProductImages", newFileName, stream);

                var imageBytes = await _fileService.ReadFileAsync("ProductImages", newFileName);

                if (imageBytes != null)
                {
                    bool isFirstImage = Images.Count == 0;

                    var imageDto = new ProductDetailsImageDto
                    {
                        Id = Guid.Empty,
                        Url = newFileName,
                        DisplayOrder = Images.Count + 1,
                        IsMain = isFirstImage,
                    };
                    var previwImage = await EditProductImageViewModel.CreateAsync(imageDto, imageBytes);
                    previwImage.SyncTags(AvailableTags);
                    Images.Add(previwImage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error uploading image: {ex.Message}");
                _toastService.ShowError("Upload Failed", "An error occurred while uploading the image.");
            }
        }

        private void SetMainImage(EditProductImageViewModel? selectedImage)
        {
            if (selectedImage == null)
                return;

            foreach (var image in Images)
            {
                image.IsMain = (image == selectedImage);
            }
        }

        private async Task DeleteImageAsync(EditProductImageViewModel? imageToDelete)
        {
            if (imageToDelete == null)
                return;

            try
            {
                // Delete physical file
                //await _fileService.DeleteFileAsync("ProductImages", imageToDelete.FileName);

                // Remove from UI
                Images.Remove(imageToDelete);

                if (imageToDelete.IsMain && Images.Count > 0)
                    Images[0].IsMain = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting image: {ex.Message}");
                _toastService.ShowError("Delete Failed", "An error occurred while deleting the image.");
            }

        }

        public List<UpdateProductImageDto> GetUpdateDto()
        {
            var updateList = new List<UpdateProductImageDto>();

            for (int i = 0; i < Images.Count; i++)
            {
                var img = Images[i];
                updateList.Add(new UpdateProductImageDto
                {
                    Id = img.Id,
                    Url = img.FileName,
                    DisplayOrder = i + 1, // Recalculate order based on UI position
                    IsMain = img.IsMain,
                    OptionName = img.SelectedTag?.OptionName,
                    OptionValue = img.SelectedTag?.OptionValue
                });
            }

            return updateList;
        }
    }

    public partial class EditProductImageViewModel : ObservableObject
    {
        public Guid Id { get; }
        public string FileName { get; }
        public BitmapImage ImageSource { get; }

        public bool _isMain;
        public bool IsMain
        {
            get => _isMain;
            set => SetProperty(ref _isMain, value);
        }

        private int _displayOrder;
        public int DisplayOrder
        {
            get => _displayOrder;
            set => SetProperty(ref _displayOrder, value);
        }

        public ObservableCollection<EditProductSelectableTagViewItem> Tags { get; } = new();

        public EditProductImageOptionTag? SelectedTag => Tags.FirstOrDefault(t => t.IsSelected)?.Tag;

        private EditProductImageViewModel(ProductDetailsImageDto imageDetails, BitmapImage imageSource)
        {
            Id = imageDetails.Id;
            FileName = imageDetails.Url;
            IsMain = imageDetails.IsMain;
            DisplayOrder = imageDetails.DisplayOrder;
            ImageSource = imageSource;
        }

        public static async Task<EditProductImageViewModel> CreateAsync(ProductDetailsImageDto imageDetails, byte[] imageBytes)
        {
            using var stream = new MemoryStream(imageBytes);
            using var randomAccessStream = stream.AsRandomAccessStream();

            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(randomAccessStream);

            return new EditProductImageViewModel(imageDetails, bitmapImage);
        }

        public void SyncTags(IEnumerable<EditProductImageOptionTag> globalTags, string? optionName = null, string? optionValue = null)
        {
            EditProductSelectableTagViewItem? prevSelected = null;
            if (SelectedTag != null)
            {
                prevSelected = new EditProductSelectableTagViewItem(SelectedTag)
                {
                    IsSelected = true
                };
            }

            if(!string.IsNullOrWhiteSpace(optionName) && !string.IsNullOrWhiteSpace(optionValue))
            {
                var selectedTag = new EditProductSelectableTagViewItem(new EditProductImageOptionTag(optionName, optionValue))
                {
                    IsSelected = true
                };
                prevSelected = selectedTag;
            }

            Tags.Clear();
            foreach(var tag in globalTags)
            {
                var tagVm = new EditProductSelectableTagViewItem(tag);
                if(prevSelected != null && tag.OptionName == prevSelected.Tag.OptionName && tag.OptionValue == prevSelected.Tag.OptionValue)
                {
                    tagVm.IsSelected = true;
                }

                tagVm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(EditProductSelectableTagViewItem.IsSelected))
                    {
                        if (tagVm.IsSelected)
                        {
                            foreach (var other in Tags.Where(x => x != tagVm))
                            {
                                other.IsSelected = false;
                            }
                        }
                        OnPropertyChanged(nameof(SelectedTag));
                    }
                };

                Tags.Add(tagVm);
            }
        }
    }

    public record EditProductImageOptionTag(string OptionName, string OptionValue)
    {
        public string DisplayLabel => $"{OptionName}: {OptionValue}";
    }

    public partial class EditProductSelectableTagViewItem : ObservableObject
    {
        public EditProductImageOptionTag Tag { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public EditProductSelectableTagViewItem(EditProductImageOptionTag tag) => Tag = tag;
    }
}
