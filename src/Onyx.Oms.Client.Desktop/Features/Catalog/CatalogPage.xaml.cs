using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Onyx.Oms.Client.Desktop.Shared.Services;

namespace Onyx.Oms.Client.Desktop.Features.Catalog;

public sealed partial class CatalogPage : Page
{
    private readonly INavigationService _navigationService;

    public CatalogViewModel ViewModel { get; }

    public CatalogPage()
    {
        InitializeComponent();
        
        ViewModel = App.Current.Services.GetRequiredService<CatalogViewModel>();
        _navigationService = App.Current.Services.GetRequiredService<INavigationService>();
        DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        ViewModel.OnNavigatedTo(e.Parameter);
    }
}
