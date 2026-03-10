using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class TagViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Tag";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public TagViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}