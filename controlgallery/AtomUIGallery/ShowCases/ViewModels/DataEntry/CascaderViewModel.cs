using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.DataLoad;
using AtomUI.Desktop.Controls.Primitives;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class CascaderViewModel : ReactiveObject, IRoutableViewModel, IActivatableViewModel
{
    public static TreeNodeKey ID = "Cascader";

    public IScreen HostScreen { get; }
    public ViewModelActivator Activator { get; }

    public string UrlPathSegment { get; } = ID.ToString();
    
    private List<ICascaderViewOption> _basicCascaderViewNodes = [];
    
    public List<ICascaderViewOption> BasicCascaderViewNodes
    {
        get => _basicCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _basicCascaderViewNodes, value);
    }
    
    private TreeNodePath? _defaultSelectItemPath;
    
    public TreeNodePath? DefaultSelectItemPath
    {
        get => _defaultSelectItemPath;
        set => this.RaiseAndSetIfChanged(ref _defaultSelectItemPath, value);
    }
    
    private List<ICascaderViewOption> _basicCheckableCascaderViewNodes = [];
    
    public List<ICascaderViewOption> BasicCheckableCascaderViewNodes
    {
        get => _basicCheckableCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _basicCheckableCascaderViewNodes, value);
    }
    
    private List<ICascaderViewOption> _hoverCascaderNodes = [];
    
    public List<ICascaderViewOption> HoverCascaderNodes
    {
        get => _hoverCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _hoverCascaderNodes, value);
    }
    
    private List<ICascaderViewOption> _disabledCascaderNodes = [];
    
    public List<ICascaderViewOption> DisabledCascaderNodes
    {
        get => _disabledCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _disabledCascaderNodes, value);
    }
    
    private List<ICascaderViewOption> _selectParentCascaderNodes = [];
    
    public List<ICascaderViewOption> SelectParentCascaderNodes
    {
        get => _selectParentCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _selectParentCascaderNodes, value);
    }
    
    private List<ICascaderViewOption> _multipleSelectCascaderNodes = [];
    
    public List<ICascaderViewOption> MultipleSelectCascaderNodes
    {
        get => _multipleSelectCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _multipleSelectCascaderNodes, value);
    }
    
    private List<ICascaderViewOption> _checkStrategyCascaderNodes = [];
    
    public List<ICascaderViewOption> CheckStrategyCascaderNodes
    {
        get => _checkStrategyCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _checkStrategyCascaderNodes, value);
    }
    
    private List<ICascaderViewOption> _prefixAndSuffixCascaderNodes = [];
    
    public List<ICascaderViewOption> PrefixAndSuffixCascaderNodes
    {
        get => _prefixAndSuffixCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _prefixAndSuffixCascaderNodes, value);
    }
    
    private SelectPopupPlacement _placement;

    public SelectPopupPlacement Placement
    {
        get => _placement;
        set => this.RaiseAndSetIfChanged(ref _placement, value);
    }
    
    private List<ICascaderViewOption> _placementCascaderNodes = [];
    
    public List<ICascaderViewOption> PlacementCascaderNodes
    {
        get => _placementCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _placementCascaderNodes, value);
    }
    
    private List<ICascaderViewOption> _searchCascaderNodes = [];
    
    public List<ICascaderViewOption> SearchCascaderNodes
    {
        get => _searchCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _searchCascaderNodes, value);
    }
    
    private List<ICascaderViewOption> _sizeCascaderNodes = [];
    
    public List<ICascaderViewOption> SizeCascaderNodes
    {
        get => _sizeCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _sizeCascaderNodes, value);
    }
    
    private List<ICascaderViewOption> _asyncLoadCascaderViewNodes = [];
    
    public List<ICascaderViewOption> AsyncLoadCascaderViewNodes
    {
        get => _asyncLoadCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _asyncLoadCascaderViewNodes, value);
    }
    
    private List<ICascaderViewOption> _searchCascaderViewNodes = [];
    
    public List<ICascaderViewOption> SearchCascaderViewNodes
    {
        get => _searchCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _searchCascaderViewNodes, value);
    }
    
    private CascaderItemDataLoader? _asyncCascaderNodeLoader;
    
    public CascaderItemDataLoader? AsyncCascaderNodeLoader
    {
        get => _asyncCascaderNodeLoader;
        set => this.RaiseAndSetIfChanged(ref _asyncCascaderNodeLoader, value);
    }
    
    private List<ICascaderViewOption> _defaultExpandCascaderViewNodes = [];
    
    public List<ICascaderViewOption> DefaultExpandCascaderViewNodes
    {
        get => _defaultExpandCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _defaultExpandCascaderViewNodes, value);
    }
    
    private TreeNodePath? _defaultExpandPath;
    
    public TreeNodePath? DefaultExpandPath
    {
        get => _defaultExpandPath;
        set => this.RaiseAndSetIfChanged(ref _defaultExpandPath, value);
    }
    
    public CascaderViewModel(IScreen screen)
    {
        HostScreen = screen;
        Activator  = new ViewModelActivator();
    }
}

public class CascaderItemDataLoader : ICascaderItemDataLoader
{
    public async Task<CascaderItemLoadResult> LoadAsync(ICascaderViewOption targetCascaderItem, CancellationToken token)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(600), token);
        var children = new List<CascaderViewOption>();
        children.AddRange([
            new CascaderViewOption()
            {
                Header = $"{targetCascaderItem.Value} Dynamic 1",
                IsLeaf = true
            },
            new CascaderViewOption()
            {
                Header = $"{targetCascaderItem.Value} Dynamic 2",
                IsLeaf = true
            }
        ]);
        return new CascaderItemLoadResult()
        {
            IsSuccess = true,
            Data      = children
        };
    }
}