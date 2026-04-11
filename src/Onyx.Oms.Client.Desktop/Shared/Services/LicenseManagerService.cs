using System;
using System.Security.Cryptography;
using System.Text;
using Windows.Security.Cryptography;
using Windows.System.Profile;

namespace Onyx.Oms.Client.Desktop.Shared.Services
{
    public class LicenseManagerService : ILicenseManagerService
    {
        private const string SecretSalt = "Onyx_OMS_Super_Secret_Master_Key_2026!";

        public string GetHardwareId()
        {
            var systemId = SystemIdentification.GetSystemIdForPublisher();
            return CryptographicBuffer.EncodeToHexString(systemId.Id);
        }

        private string GenerateValidKeyString(string hardwareId)
        {
            // Combine the ID and your secret salt, then hash it so it can't be reversed
            string rawData = hardwareId + SecretSalt;

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return Convert.ToBase64String(bytes);
            }
        }

        public bool IsKeyValid(string fileContents)
        {
            string currentHardwareId = GetHardwareId();
            string expectedKey = GenerateValidKeyString(currentHardwareId);

            return fileContents.Trim() == expectedKey;
        }
    }
}
