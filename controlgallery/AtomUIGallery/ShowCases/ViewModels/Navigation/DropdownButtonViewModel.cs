using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class DropdownButtonViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "DropdownButton";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public DropdownButtonViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}