using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Collections;
using Avalonia.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class TabControlViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "TabControl";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    #region TabStrip

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

    private SizeType _sizeTypeTabStrip = SizeType.Middle;

    public SizeType SizeTypeTabStrip
    {
        get => _sizeTypeTabStrip;
        set => this.RaiseAndSetIfChanged(ref _sizeTypeTabStrip, value);
    }

    public AvaloniaList<TabItemData> TabStripItemDataSource { get; set; } = new();

    #endregion

    #region TabControl

    private Dock _positionTabControlPlacement = Dock.Top;

    public Dock PositionTabControlPlacement
    {
        get => _positionTabControlPlacement;
        set => this.RaiseAndSetIfChanged(ref _positionTabControlPlacement, value);
    }

    private Dock _positionCardTabControlPlacement = Dock.Top;

    public Dock PositionCardTabControlPlacement
    {
        get => _positionCardTabControlPlacement;
        set => this.RaiseAndSetIfChanged(ref _positionCardTabControlPlacement, value);
    }

    private SizeType _sizeTypeControl = SizeType.Middle;

    public SizeType SizeTypeTabControl
    {
        get => _sizeTypeControl;
        set => this.RaiseAndSetIfChanged(ref _sizeTypeControl, value);
    }

    public AvaloniaList<TabItemData> TabItemDataSource { get; set; } = new();
    #endregion

    public TabControlViewModel(IScreen screen)
    {
        HostScreen = screen;
    }

    #region TabStrip

    public void HandleTabStripPlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (args.Index == 0)
        {
            PositionTabStripPlacement = Dock.Top;
        }
        else if (args.Index == 1)
        {
            PositionTabStripPlacement = Dock.Bottom;
        }
        else if (args.Index == 2)
        {
            PositionTabStripPlacement = Dock.Left;
        }
        else
        {
            PositionTabStripPlacement = Dock.Right;
        }
    }

    public void HandleCardTabStripPlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (args.Index == 0)
        {
            PositionCardTabStripPlacement = Dock.Top;
        }
        else if (args.Index == 1)
        {
            PositionCardTabStripPlacement = Dock.Bottom;
        }
        else if (args.Index == 2)
        {
            PositionCardTabStripPlacement = Dock.Left;
        }
        else
        {
            PositionCardTabStripPlacement = Dock.Right;
        }
    }

    public void HandleTabStripSizeTypeOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (args.Index == 0)
        {
            SizeTypeTabStrip = SizeType.Small;
        }
        else if (args.Index == 1)
        {
            SizeTypeTabStrip = SizeType.Middle;
        }
        else
        {
            SizeTypeTabStrip = SizeType.Large;
        }
    }

    #endregion

    #region TabControl

    public void HandleTabControlPlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (args.Index == 0)
        {
            PositionTabControlPlacement = Dock.Top;
        }
        else if (args.Index == 1)
        {
            PositionTabControlPlacement = Dock.Bottom;
        }
        else if (args.Index == 2)
        {
            PositionTabControlPlacement = Dock.Left;
        }
        else
        {
            PositionTabControlPlacement = Dock.Right;
        }
    }

    public void HandleCardTabControlPlacementOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (args.Index == 0)
        {
            PositionCardTabControlPlacement = Dock.Top;
        }
        else if (args.Index == 1)
        {
            PositionCardTabControlPlacement = Dock.Bottom;
        }
        else if (args.Index == 2)
        {
            PositionCardTabControlPlacement = Dock.Left;
        }
        else
        {
            PositionCardTabControlPlacement = Dock.Right;
        }
    }

    public void HandleTabControlSizeTypeOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (args.Index == 0)
        {
            SizeTypeTabControl = SizeType.Small;
        }
        else if (args.Index == 1)
        {
            SizeTypeTabControl = SizeType.Middle;
        }
        else
        {
            SizeTypeTabControl = SizeType.Large;
        }
    }

    #endregion
}
