using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Collections;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class FormViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Form";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private FormLayout _formLayout = FormLayout.Horizontal;

    public FormLayout FormLayout
    {
        get => _formLayout;
        set => this.RaiseAndSetIfChanged(ref _formLayout, value);
    }

    private bool _isFormDisabled = true;

    public bool IsFormDisabled
    {
        get => _isFormDisabled;
        set => this.RaiseAndSetIfChanged(ref _isFormDisabled, value);
    }
    
    private InputControlStyleVariant _formStyleVariant = InputControlStyleVariant.Outline;

    public InputControlStyleVariant FormStyleVariant
    {
        get => _formStyleVariant;
        set => this.RaiseAndSetIfChanged(ref _formStyleVariant, value);
    }
    
    private FormRequiredMark _formRequiredMark = FormRequiredMark.Default;

    public FormRequiredMark FormRequiredMark
    {
        get => _formRequiredMark;
        set => this.RaiseAndSetIfChanged(ref _formRequiredMark, value);
    }
    
    private SizeType _formSizeType = SizeType.Middle;

    public SizeType FormSizeType
    {
        get => _formSizeType;
        set => this.RaiseAndSetIfChanged(ref _formSizeType, value);
    }
    
    private List<SliderMark>? _sliderMarks;

    public List<SliderMark>? SliderMarks
    {
        get => _sliderMarks;
        set => this.RaiseAndSetIfChanged(ref _sliderMarks, value);
    }
    
    public FormViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}