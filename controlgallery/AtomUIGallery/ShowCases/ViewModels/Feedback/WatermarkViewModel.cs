using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class WatermarkViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Watermark";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public WatermarkViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}