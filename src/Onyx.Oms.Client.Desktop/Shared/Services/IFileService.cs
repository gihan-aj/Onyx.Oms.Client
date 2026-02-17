using System.IO;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public interface IFileService
{
    /// <summary>
    /// Resizes the image stream to a max dimension (e.g. 800px) and saves it to the specified folder.
    /// Returns the saved file name (which might be renamed/generated).
    /// </summary>
    Task<string> SaveImageAsync(string folderName, string fileName, Stream imageStream);

    /// <summary>
    /// Reads a file into a byte array, ensuring the file handle is closed immediately.
    /// Safe for use with BitmapImage sources to prevent locking.
    /// </summary>
    Task<byte[]?> ReadFileAsync(string folderName, string fileName);

    Task DeleteFileAsync(string folderName, string fileName);
}
