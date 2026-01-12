using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class MentionsViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "Mentions";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public MentionsViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}