using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class LineEditViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "LineEdit";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public LineEditViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}