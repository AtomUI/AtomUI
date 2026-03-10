using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class ColorPickerViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "ColorPicker";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public ColorPickerViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}