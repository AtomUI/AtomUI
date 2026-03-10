using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class BoxPanelViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "BoxPanelShowCase";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public BoxPanelViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}