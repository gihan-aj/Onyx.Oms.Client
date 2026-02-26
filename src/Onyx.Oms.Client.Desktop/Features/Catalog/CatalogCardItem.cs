using CommunityToolkit.Mvvm.ComponentModel;

namespace Onyx.Oms.Client.Desktop.Features.Catalog;

public partial class CatalogCardItem : ObservableObject
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    
    private string _metricValue = string.Empty;
    public string MetricValue
    {
        get => _metricValue;
        set => SetProperty(ref _metricValue, value);
    }
    
    public string IconGlyph { get; init; } = string.Empty;
    
    public string TargetPageType { get; init; } = string.Empty;
}
