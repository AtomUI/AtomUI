using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class SplitButtonViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "SplitButton";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public SplitButtonViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}