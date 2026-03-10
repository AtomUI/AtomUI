using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class InfoFlyoutViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "InfoFlyout";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    private bool _showArrow = true;

    public bool ShowArrow
    {
        get => _showArrow;
        set => this.RaiseAndSetIfChanged(ref _showArrow, value);
    }

    private bool _isPointAtCenter;

    public bool IsPointAtCenter
    {
        get => _isPointAtCenter;
        set => this.RaiseAndSetIfChanged(ref _isPointAtCenter, value);
    }

    public InfoFlyoutViewModel(IScreen screen)
    {
        HostScreen = screen;
    }

    public void HandleSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        if (sender is Segmented segmented)
        {
            if (segmented.SelectedIndex == 0)
            {
                ShowArrow       = true;
                IsPointAtCenter = false;
            }
            else if (segmented.SelectedIndex == 1)
            {
                ShowArrow       = false;
                IsPointAtCenter = false;
            }
            else if (segmented.SelectedIndex == 2)
            {
                IsPointAtCenter = true;
                ShowArrow       = true;
            }
        }
    }
}