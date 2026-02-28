using Refit;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Features.Settings.Services;

public interface IAppSequenceApi
{
    [Get("/api/v1/settings/sequences/{id}")]
    Task<int> GetSequenceValue(string id);

    [Put("/api/v1/settings/sequences/{id}")]
    Task UpdateSequenceValue(string id, [Body] int value);
}
