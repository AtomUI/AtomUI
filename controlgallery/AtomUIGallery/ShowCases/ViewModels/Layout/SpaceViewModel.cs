using AtomUI;
using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class SpaceViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "SpaceShowCase";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();
    
    private CustomizableSizeType _sizeType;

    public CustomizableSizeType SizeType
    {
        get => _sizeType;
        set => this.RaiseAndSetIfChanged(ref _sizeType, value);
    }
    
    private double _customSpacingValue;

    public double CustomSpacingValue
    {
        get => _customSpacingValue;
        set => this.RaiseAndSetIfChanged(ref _customSpacingValue, value);
    }

    public SpaceViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}
