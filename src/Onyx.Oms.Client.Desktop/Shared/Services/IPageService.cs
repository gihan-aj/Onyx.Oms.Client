using Microsoft.UI.Xaml.Controls;
using System;

namespace Onyx.Oms.Client.Desktop.Shared.Services;

public interface IPageService
{
    Type GetPageType(string key);
    void Configure<VM, V>() where VM : class where V : Page;
    void Configure(string key, Type pageType);
}
