using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class NotificationViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Notification";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public NotificationViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}