using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class AlertViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Alert";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public AlertViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}