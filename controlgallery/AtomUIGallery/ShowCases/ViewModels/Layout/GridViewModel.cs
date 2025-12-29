using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class GridViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "GridShowCase";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    public GridViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}
