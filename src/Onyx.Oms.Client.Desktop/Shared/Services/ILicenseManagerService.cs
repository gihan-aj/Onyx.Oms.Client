namespace Onyx.Oms.Client.Desktop.Shared.Services
{
    public interface ILicenseManagerService
    {
        string GetHardwareId();
        bool IsKeyValid(string fileContents);
    }
}