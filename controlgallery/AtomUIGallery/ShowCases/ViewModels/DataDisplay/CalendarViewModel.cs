using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class CalendarViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Calendar";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    public CalendarViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}