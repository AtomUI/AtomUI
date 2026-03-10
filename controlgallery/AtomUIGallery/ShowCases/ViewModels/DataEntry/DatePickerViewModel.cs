using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class DatePickerViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "DatePicker";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    private SizeType _pickerSizeType = SizeType.Middle;

    public SizeType PickerSizeType
    {
        get => _pickerSizeType;
        set => this.RaiseAndSetIfChanged(ref _pickerSizeType, value);
    }

    private PlacementMode _pickerPlacement;

    public PlacementMode PickerPlacement
    {
        get => _pickerPlacement;
        set => this.RaiseAndSetIfChanged(ref _pickerPlacement, value);
    }

    public DatePickerViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
    
    public void HandlePickerSizeTypeOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (args.Index == 0)
        {
            PickerSizeType = SizeType.Large;
        }
        else if (args.Index == 1)
        {
            PickerSizeType = SizeType.Middle;
        }
        else
        {
            PickerSizeType = SizeType.Small;
        }
    }
    
    public void HandlePickerPlacementCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (args.Index == 0)
        {
            PickerPlacement = PlacementMode.TopEdgeAlignedLeft;
        }
        else if (args.Index == 1)
        {
            PickerPlacement = PlacementMode.TopEdgeAlignedRight;
        }
        else if (args.Index == 2)
        {
            PickerPlacement = PlacementMode.BottomEdgeAlignedLeft;
        }
        else
        {
            PickerPlacement = PlacementMode.BottomEdgeAlignedRight;
        }
    }
}