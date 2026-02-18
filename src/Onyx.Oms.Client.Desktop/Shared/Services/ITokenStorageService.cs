using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public interface ITokenStorageService
{
    Task SaveTokensAsync(string accessToken, string refreshToken, string idToken);
    Task<(string? AccessToken, string? RefreshToken, string? IdToken)> GetTokensAsync();
    Task ClearTokensAsync();
}
