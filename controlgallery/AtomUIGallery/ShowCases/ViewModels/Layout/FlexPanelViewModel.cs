using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class FlexPanelViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "FlexPanelShowCase";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    public FlexPanelViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}
