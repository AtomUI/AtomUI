using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Collections;
using Avalonia.Media;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class SliderViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Slider";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    private List<SliderMark>? _sliderMarks;

    public List<SliderMark>? SliderMarks
    {
        get => _sliderMarks;
        set => this.RaiseAndSetIfChanged(ref _sliderMarks, value);
    }

    private bool _normalEnabled = true;

    public bool NormalEnabled
    {
        get => _normalEnabled;
        set => this.RaiseAndSetIfChanged(ref _normalEnabled, value);
    }
        
    public SliderViewModel(IScreen screen)
    {
        HostScreen  = screen;
    }
}