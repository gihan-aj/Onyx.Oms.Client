using CommunityToolkit.Mvvm.Input;

namespace Onyx.Oms.Client.Desktop.Features.Catalog;

public record CatalogNavigationItem(string Title, string Description, string IconGlyph, IRelayCommand Command);
