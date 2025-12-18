using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class UploadViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "Upload";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public UploadViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}