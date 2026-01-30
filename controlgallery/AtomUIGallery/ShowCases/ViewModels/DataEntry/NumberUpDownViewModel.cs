using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class NumberUpDownViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "NumberUpDown";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    private string? _stringModeValue = "0.123456789012345678901234";

    public string? StringModeValue
    {
        get => _stringModeValue;
        set => this.RaiseAndSetIfChanged(ref _stringModeValue, value);
    }

    private bool _keyboardEnabled = true;
    public bool KeyboardEnabled
    {
        get => _keyboardEnabled;
        set => this.RaiseAndSetIfChanged(ref _keyboardEnabled, value);
    }

    private bool _mouseWheelEnabled = true;
    public bool MouseWheelEnabled
    {
        get => _mouseWheelEnabled;
        set => this.RaiseAndSetIfChanged(ref _mouseWheelEnabled, value);
    }

    public NumberUpDownViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}
