using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class CollapseViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Collapse";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    private CollapseExpandIconPosition _collapseExpandIconPosition;

    public CollapseExpandIconPosition CollapseExpandIconPosition
    {
        get => _collapseExpandIconPosition;
        set => this.RaiseAndSetIfChanged(ref _collapseExpandIconPosition, value);
    }

    public CollapseViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
    
    public void HandleExpandButtonPosOptionCheckedChanged(object? sender, OptionCheckedChangedEventArgs args)
    {
        if (args.Index == 0)
        {
            CollapseExpandIconPosition = CollapseExpandIconPosition.Start;
        }
        else if (args.Index == 1)
        {
            CollapseExpandIconPosition = CollapseExpandIconPosition.End;
        }
    }
}