using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}
