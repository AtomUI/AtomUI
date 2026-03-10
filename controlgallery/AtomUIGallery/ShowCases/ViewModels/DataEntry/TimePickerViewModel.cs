using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class TimePickerViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "TimePicker";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    public TimePickerViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}