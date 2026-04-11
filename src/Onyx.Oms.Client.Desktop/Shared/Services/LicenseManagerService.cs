using System;
using System.Security.Cryptography;
using System.Text;
using Windows.Security.Cryptography;
using Windows.System.Profile;

namespace Onyx.Oms.Client.Desktop.Shared.Services
{
    public class LicenseManagerService : ILicenseManagerService
    {
        private const string PublicKey = "<RSAKeyValue><Modulus>uGPP/a/doEy4iO95MqN/7lOIRm0QAfnvYOI6tGtixHpb6f7yg94BfNGl7e8BwnmQ2e7xyvuSZo5nCSTjCmyfT2Fz2emYtAqLbmQ4jT6QWLcq1RsjdnW24BS40ILlWRUDs1HR0C8Y2xuX5bGl1jrJBG9HCUUkG6lf1r2Rz84GZG/CzcHt0jZvxGRG9eWhytJdntMBSKXL5OlviTGocBebZ/rmnPGdt5S8oK/JZcKTQ68nRjCc8QlFpH46wNBXe60PqHtcOmEiAsq+w355oMux3Jq4+Rm4sDYrhQJvJTYDtN6F8M3J976y2GJ/CgsLzhh3NbvByp2g2gEF/I9Iv0EkTQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

        public string GetHardwareId()
        {
            var systemId = SystemIdentification.GetSystemIdForPublisher();
            return CryptographicBuffer.EncodeToHexString(systemId.Id);
        }

        public bool IsKeyValid(string fileContents)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileContents)) return false;

                string currentHardwareId = GetHardwareId();
                byte[] hardwareIdBytes = Encoding.UTF8.GetBytes(currentHardwareId);
                byte[] signatureBytes = Convert.FromBase64String(fileContents.Trim());

                using (var rsa = RSA.Create())
                {
                    rsa.FromXmlString(PublicKey);
                    // Attempt to verify the RSA PKCS#1 signature
                    return rsa.VerifyData(hardwareIdBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
            }
            catch
            {
                // If it fails to parse base64, or crashes on verify, it's an invalid/forged key
                return false;
            }
        }
    }
}
