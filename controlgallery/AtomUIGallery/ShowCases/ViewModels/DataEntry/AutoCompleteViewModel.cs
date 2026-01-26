using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class AutoCompleteViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{
    public static TreeNodeKey ID = "AutoComplete";
    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; }

    public string UrlPathSegment { get; } = ID.ToString();
    
    public AutoCompleteViewModel(IScreen screen)
    {
        HostScreen = screen;
        Activator  = new ViewModelActivator();
    }
}
