using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class RadioButtonViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "RadioButton";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private IList<RadioButtonOption>? _radioOptions;

    public IList<RadioButtonOption>? RadioOptions
    {
        get => _radioOptions;
        set => this.RaiseAndSetIfChanged(ref _radioOptions, value);
    }

    public RadioButtonViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}