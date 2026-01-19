using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class CascaderViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{
    public static TreeNodeKey ID = "Cascader";

    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; }

    public string UrlPathSegment { get; } = ID.ToString();
    
    private List<ICascaderViewItemData> _basicCascaderNodes = [];
    
    public List<ICascaderViewItemData> BasicCascaderNodes
    {
        get => _basicCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _basicCascaderNodes, value);
    }
    
    private List<ICascaderViewItemData> _basicCheckableCascaderNodes = [];
    
    public List<ICascaderViewItemData> BasicCheckableCascaderNodes
    {
        get => _basicCheckableCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _basicCheckableCascaderNodes, value);
    }
    
    public CascaderViewModel(IScreen screen)
    {
        HostScreen = screen;
        Activator  = new ViewModelActivator();
    }
}