using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class DrawerViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "Drawer";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private DrawerPlacement _multiLevelPlacement = DrawerPlacement.Right;

    public DrawerPlacement MultiLevelPlacement
    {
        get => _multiLevelPlacement;
        set => this.RaiseAndSetIfChanged(ref _multiLevelPlacement, value);
    }
    
    private DrawerPlacement _extraAndFooterPlacement = DrawerPlacement.Right;

    public DrawerPlacement ExtraAndFooterPlacement
    {
        get => _extraAndFooterPlacement;
        set => this.RaiseAndSetIfChanged(ref _extraAndFooterPlacement, value);
    }
    
    private DrawerPlacement _customPlacement = DrawerPlacement.Right;

    public DrawerPlacement CustomPlacement
    {
        get => _customPlacement;
        set => this.RaiseAndSetIfChanged(ref _customPlacement, value);
    }
    public DrawerViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}