namespace Onyx.Oms.Client.Desktop.Shared.Services;

public interface INavigationAware
{
    void OnNavigatedTo(object parameter);
    void OnNavigatedFrom();
}
