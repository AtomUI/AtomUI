using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class IconViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Icon";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public IconViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}