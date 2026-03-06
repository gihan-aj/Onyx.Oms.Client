using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Products;

public partial class ImagePreviewModel : ObservableObject
{
    public string FileName { get; } = string.Empty;

    // The in-memory image
    public BitmapImage ImageSource { get; } = null!;

    private bool _isMain;
    public bool IsMain
    {
        get => _isMain;
        set
        {
            SetProperty(ref _isMain, value);
        }
    }

    private int _displayOrder;
    public int DisplayOrder     
    {
        get => _displayOrder;
        set
        {
            SetProperty(ref _displayOrder, value);
        }
    }

    public ImageOptionTag? SelectedTag => AvailableTags.FirstOrDefault(t => t.IsSelected)?.Tag;

    public ImagePreviewModel(string fileName, BitmapImage imageSource, bool isMain)
    {
        FileName = fileName;
        ImageSource = imageSource;
        IsMain = isMain;
    }

    public ObservableCollection<SelectableTagViewModel> AvailableTags { get; } = new();

    public static async Task<ImagePreviewModel> CreateAsync(string fileName, byte[] imageBytes, bool isMain)
    {
        using var stream = new MemoryStream(imageBytes);
        using var randomAccessStream = stream.AsRandomAccessStream();

        var bitmapImage = new BitmapImage();
        await bitmapImage.SetSourceAsync(randomAccessStream);

        return new ImagePreviewModel(fileName, bitmapImage, isMain);
    }

    public void SyncTags(IEnumerable<ImageOptionTag> globalTags)
    {
        var previouslySelected = SelectedTag;
        AvailableTags.Clear();

        foreach(var tag in globalTags)
        {
            var tagVm = new SelectableTagViewModel(tag);

            // Restore previous selection if the option wasn't deleted
            if (previouslySelected != null && tag.OptionName == previouslySelected.OptionName && tag.OptionValue == previouslySelected.OptionValue)
            {
                tagVm.IsSelected = true;
            }

            // Enforce single selection (Radio Button behavior)
            tagVm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SelectableTagViewModel.IsSelected) && tagVm.IsSelected)
                {
                    // Uncheck all OTHER tags for this specific image
                    foreach (var other in AvailableTags.Where(x => x != tagVm))
                    {
                        other.IsSelected = false;
                    }
                }
            };

            AvailableTags.Add(tagVm);
        }
    }
}
