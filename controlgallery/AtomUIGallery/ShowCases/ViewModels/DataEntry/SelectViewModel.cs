using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class SelectViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "Select";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private List<SelectOption>? _randomOptions;

    public List<SelectOption>? RandomOptions
    {
        get => _randomOptions;
        set => this.RaiseAndSetIfChanged(ref _randomOptions, value);
    }
    
    private List<ISelectOption> _basicSelectedOptions = [];

    public List<ISelectOption> BasicSelectedOptions
    {
        get => _basicSelectedOptions;
        set => this.RaiseAndSetIfChanged(ref _basicSelectedOptions, value);
    }
    
    private List<ISelectOption>? _defaultSelectedOptions;

    public List<ISelectOption>? DefaultSelectedOptions
    {
        get => _defaultSelectedOptions;
        set => this.RaiseAndSetIfChanged(ref _defaultSelectedOptions, value);
    }
    
    private List<SelectOption>? _maxTagCountOptions;

    public List<SelectOption>? MaxTagCountOptions
    {
        get => _maxTagCountOptions;
        set => this.RaiseAndSetIfChanged(ref _maxTagCountOptions, value);
    }
    
    private SizeType _selectSizeType;

    public SizeType SelectSizeType
    {
        get => _selectSizeType;
        set => this.RaiseAndSetIfChanged(ref _selectSizeType, value);
    }
        
    public SelectViewModel(IScreen screen)
    {
        HostScreen  = screen;
    }
}