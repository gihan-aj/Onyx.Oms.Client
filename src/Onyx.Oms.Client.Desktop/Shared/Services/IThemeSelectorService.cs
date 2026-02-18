using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public interface IThemeSelectorService
{
    ElementTheme Theme { get; }
    Task SetThemeAsync(ElementTheme theme);
    Task SetRequestedThemeAsync();
}
