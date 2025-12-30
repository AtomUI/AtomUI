using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class SplitterViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "SplitterShowCase";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    public SplitterViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}
