using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class SpaceViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "SpaceShowCase";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    public SpaceViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}
