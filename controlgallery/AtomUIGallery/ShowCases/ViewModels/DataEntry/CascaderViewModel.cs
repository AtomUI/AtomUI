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
    
    private List<ICascaderOption> _basicCascaderViewNodes = [];
    
    public List<ICascaderOption> BasicCascaderViewNodes
    {
        get => _basicCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _basicCascaderViewNodes, value);
    }
    
    private TreeNodePath? _defaultSelectOptionPath;
    
    public TreeNodePath? DefaultSelectOptionPath
    {
        get => _defaultSelectOptionPath;
        set => this.RaiseAndSetIfChanged(ref _defaultSelectOptionPath, value);
    }
    
    private List<ICascaderOption> _basicCheckableCascaderViewNodes = [];
    
    public List<ICascaderOption> BasicCheckableCascaderViewNodes
    {
        get => _basicCheckableCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _basicCheckableCascaderViewNodes, value);
    }
    
    private List<ICascaderOption> _hoverCascaderNodes = [];
    
    public List<ICascaderOption> HoverCascaderNodes
    {
        get => _hoverCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _hoverCascaderNodes, value);
    }
    
    private List<ICascaderOption> _disabledCascaderNodes = [];
    
    public List<ICascaderOption> DisabledCascaderNodes
    {
        get => _disabledCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _disabledCascaderNodes, value);
    }
    
    private List<ICascaderOption> _selectParentCascaderNodes = [];
    
    public List<ICascaderOption> SelectParentCascaderNodes
    {
        get => _selectParentCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _selectParentCascaderNodes, value);
    }
    
    private List<ICascaderOption> _multipleSelectCascaderNodes = [];
    
    public List<ICascaderOption> MultipleSelectCascaderNodes
    {
        get => _multipleSelectCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _multipleSelectCascaderNodes, value);
    }
    
    private List<ICascaderOption> _checkStrategyShowParentCascaderNodes = [];
    
    public List<ICascaderOption> CheckStrategyShowParentCascaderNodes
    {
        get => _checkStrategyShowParentCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _checkStrategyShowParentCascaderNodes, value);
    }
    
    private List<ICascaderOption> _checkStrategyShowAllCascaderNodes = [];
    
    public List<ICascaderOption> CheckStrategyShowAllCascaderNodes
    {
        get => _checkStrategyShowAllCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _checkStrategyShowAllCascaderNodes, value);
    }
    
    private List<ICascaderOption> _prefixAndSuffixCascaderNodes = [];
    
    public List<ICascaderOption> PrefixAndSuffixCascaderNodes
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
    
    private List<ICascaderOption> _placementCascaderNodes = [];
    
    public List<ICascaderOption> PlacementCascaderNodes
    {
        get => _placementCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _placementCascaderNodes, value);
    }
    
    private List<ICascaderOption> _searchCascaderNodes = [];
    
    public List<ICascaderOption> SearchCascaderNodes
    {
        get => _searchCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _searchCascaderNodes, value);
    }
    
    private List<ICascaderOption> _sizeCascaderNodes = [];
    
    public List<ICascaderOption> SizeCascaderNodes
    {
        get => _sizeCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _sizeCascaderNodes, value);
    }
    
    private List<ICascaderOption> _asyncLoadCascaderViewNodes = [];
    
    public List<ICascaderOption> AsyncLoadCascaderViewNodes
    {
        get => _asyncLoadCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _asyncLoadCascaderViewNodes, value);
    }
    
    private List<ICascaderOption> _searchCascaderViewNodes = [];
    
    public List<ICascaderOption> SearchCascaderViewNodes
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
    
    private List<ICascaderOption> _defaultExpandCascaderViewNodes = [];
    
    public List<ICascaderOption> DefaultExpandCascaderViewNodes
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
    public async Task<CascaderItemLoadResult> LoadAsync(ICascaderOption targetCascaderItem, CancellationToken token)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(600), token);
        var children = new List<CascaderOption>();
        children.AddRange([
            new CascaderOption()
            {
                Header = $"{targetCascaderItem.Value} Dynamic 1",
                IsLeaf = true
            },
            new CascaderOption()
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