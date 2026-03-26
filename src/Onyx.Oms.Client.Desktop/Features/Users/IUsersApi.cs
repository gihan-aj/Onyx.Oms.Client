using Refit;
using System;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Users
{
    public interface IUsersApi
    {
        [Post("/api/v1/users")]
        Task<Guid> RegisterUserAsync([Body]RegisterUserRequest request);
    }
}
