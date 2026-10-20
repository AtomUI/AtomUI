using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Splash;

public class SplashViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Splash";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    public SplashViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}
