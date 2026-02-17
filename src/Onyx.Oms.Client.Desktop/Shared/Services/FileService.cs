using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public class FileService : IFileService
{
    private const uint MaxDimension = 800; // Max width or height in pixels

    public async Task<string> SaveImageAsync(string folderName, string fileName, Stream imageStream)
    {
        var localFolder = ApplicationData.Current.LocalFolder;
        var folder = await localFolder.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists);
        
        // Ensure unique filename or use provided one
        // User requested to control the name (e.g. productId_guid)
        var storageFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

        // Reset stream position if needed
        if (imageStream.Position > 0) imageStream.Seek(0, SeekOrigin.Begin);

        using var outputStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite);
        using var randomAccessStream = imageStream.AsRandomAccessStream();

        // Decode
        var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);

        // Resize logic
        var transform = new BitmapTransform();
        if (decoder.PixelWidth > MaxDimension || decoder.PixelHeight > MaxDimension)
        {
            float ratio = Math.Min((float)MaxDimension / decoder.PixelWidth, (float)MaxDimension / decoder.PixelHeight);
            transform.ScaledWidth = (uint)(decoder.PixelWidth * ratio);
            transform.ScaledHeight = (uint)(decoder.PixelHeight * ratio);
        }

        // Encode and Save
        // CreateForTranscodingAsync automatically matches the encoder to the decoder
        var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);
        
        encoder.BitmapTransform.ScaledWidth = transform.ScaledWidth;
        encoder.BitmapTransform.ScaledHeight = transform.ScaledHeight;

        try
        {
            await encoder.FlushAsync();
        }
        catch (Exception)
        {
            // Fallback if transcoding fails (e.g. format mismatch), just copy properties
            // But usually CreateForTranscoding handles this well if we match IDs
            throw;
        }

        return fileName;
    }

    public async Task<byte[]?> ReadFileAsync(string folderName, string fileName)
    {
        var localFolder = ApplicationData.Current.LocalFolder;
        try
        {
            var folder = await localFolder.GetFolderAsync(folderName);
            var file = await folder.GetFileAsync(fileName);

            // Read to byte array to release lock immediately
            using var stream = await file.OpenStreamForReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    public async Task DeleteFileAsync(string folderName, string fileName)
    {
        var localFolder = ApplicationData.Current.LocalFolder;
        try
        {
            var folder = await localFolder.GetFolderAsync(folderName);
            var file = await folder.GetFileAsync(fileName);
            await file.DeleteAsync();
        }
        catch (FileNotFoundException)
        {
            // Already deleted
        }
    }
}
