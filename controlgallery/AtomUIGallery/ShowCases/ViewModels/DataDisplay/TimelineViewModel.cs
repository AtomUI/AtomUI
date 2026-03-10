using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class TimelineViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Timeline";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public TimelineViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}