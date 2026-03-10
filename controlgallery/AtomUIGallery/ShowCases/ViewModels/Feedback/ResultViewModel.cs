using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class ResultViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Result";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public ResultViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}