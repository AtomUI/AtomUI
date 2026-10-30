using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Collections;
using Avalonia.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.TabStrip;

public class TabStripViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "TabStrip";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    private Dock _positionTabStripPlacement = Dock.Top;

    public Dock PositionTabStripPlacement
    {
        get => _positionTabStripPlacement;
        set => this.RaiseAndSetIfChanged(ref _positionTabStripPlacement, value);
    }

    private Dock _positionCardTabStripPlacement = Dock.Top;

    public Dock PositionCardTabStripPlacement
    {
        get => _positionCardTabStripPlacement;
        set => this.RaiseAndSetIfChanged(ref _positionCardTabStripPlacement, value);
    }

    private Dock _reorderTabStripPlacement = Dock.Top;

    public Dock ReorderTabStripPlacement
    {
        get => _reorderTabStripPlacement;
        set => this.RaiseAndSetIfChanged(ref _reorderTabStripPlacement, value);
    }

    private SizeType _sizeTypeTabStrip = SizeType.Middle;

    public SizeType SizeTypeTabStrip
    {
        get => _sizeTypeTabStrip;
        set => this.RaiseAndSetIfChanged(ref _sizeTypeTabStrip, value);
    }

    public AvaloniaList<TabItemData> TabStripItemDataSource { get; set; } = new();

    public AvaloniaList<TabItemData> OverflowTabItems { get; } = [];

    public TabStripViewModel(IScreen screen)
    {
        HostScreen = screen;
        for (var index = 0; index < 30; index++)
        {
            OverflowTabItems.Add(new TabItemData
            {
                Header = $"Tab-{index}",
                IsEnabled = index != 28
            });
        }
    }

    public void HandlePlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        PositionTabStripPlacement = args.Index switch
        {
            0 => Dock.Top,
            1 => Dock.Bottom,
            2 => Dock.Left,
            _ => Dock.Right
        };
    }

    public void HandleCardPlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        PositionCardTabStripPlacement = args.Index switch
        {
            0 => Dock.Top,
            1 => Dock.Bottom,
            2 => Dock.Left,
            _ => Dock.Right
        };
    }

    public void HandleReorderPlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        ReorderTabStripPlacement = args.Index switch
        {
            0 => Dock.Top,
            1 => Dock.Bottom,
            2 => Dock.Left,
            _ => Dock.Right
        };
    }

    public void HandleSizeTypeOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        SizeTypeTabStrip = args.Index switch
        {
            0 => SizeType.Small,
            1 => SizeType.Middle,
            _ => SizeType.Large
        };
    }

}
