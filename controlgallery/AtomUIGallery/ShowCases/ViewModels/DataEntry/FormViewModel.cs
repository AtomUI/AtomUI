using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class FormViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "Form";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    public FormViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}