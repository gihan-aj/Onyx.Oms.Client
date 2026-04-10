using CommunityToolkit.Mvvm.Input;

namespace Onyx.Oms.Client.Desktop.Features.Dashboard;

public record DashboardQuickAction(
    string Title, 
    string Description, 
    string IconGlyph,
    IRelayCommand Command,
    bool IsEnabled,
    bool IsVisible
);
