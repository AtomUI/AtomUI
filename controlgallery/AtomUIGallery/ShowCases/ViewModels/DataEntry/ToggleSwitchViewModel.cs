using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class ToggleSwitchViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "ToggleSwitch";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public ToggleSwitchViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}