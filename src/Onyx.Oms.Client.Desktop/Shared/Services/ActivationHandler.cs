using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public interface IActivationHandler
{
    bool CanHandle(object args);
    Task HandleAsync(object args);
}

public abstract class ActivationHandler<T> : IActivationHandler where T : class
{
    public bool CanHandle(object args) => args is T && CanHandleInternal((args as T)!);

    public async Task HandleAsync(object args) => await HandleInternalAsync((args as T)!);

    protected virtual bool CanHandleInternal(T args) => true;

    protected abstract Task HandleInternalAsync(T args);
}
