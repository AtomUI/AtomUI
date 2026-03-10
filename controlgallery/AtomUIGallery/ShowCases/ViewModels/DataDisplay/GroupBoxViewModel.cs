using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class GroupBoxViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "GroupBox";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public GroupBoxViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}