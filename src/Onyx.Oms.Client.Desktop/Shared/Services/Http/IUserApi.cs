using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services.Http;

public interface IUserApi
{
    [Get("/api/v1/users/me/permissions")]
    Task<List<string>> GetUserPermissionsAsync();
}
