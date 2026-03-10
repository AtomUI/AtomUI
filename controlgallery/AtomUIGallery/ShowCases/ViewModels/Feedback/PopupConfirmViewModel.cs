using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class PopupConfirmViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "PopupConfirm";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public PopupConfirmViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}