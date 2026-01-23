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
    
    private List<ICascaderViewItemData> _basicCascaderViewNodes = [];
    
    public List<ICascaderViewItemData> BasicCascaderViewNodes
    {
        get => _basicCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _basicCascaderViewNodes, value);
    }
    
    private List<ICascaderViewItemData> _basicCascaderNodes = [];
    
    public List<ICascaderViewItemData> BasicCascaderNodes
    {
        get => _basicCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _basicCascaderNodes, value);
    }
    
    private List<ICascaderViewItemData> _defaultValueCascaderNodes = [];
    
    public List<ICascaderViewItemData> DefaultValueCascaderNodes
    {
        get => _defaultValueCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _defaultValueCascaderNodes, value);
    }
    
    private TreeNodePath? _defaultSelectItemPath;
    
    public TreeNodePath? DefaultSelectItemPath
    {
        get => _defaultSelectItemPath;
        set => this.RaiseAndSetIfChanged(ref _defaultSelectItemPath, value);
    }
    
    private List<ICascaderViewItemData> _basicCheckableCascaderViewNodes = [];
    
    public List<ICascaderViewItemData> BasicCheckableCascaderViewNodes
    {
        get => _basicCheckableCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _basicCheckableCascaderViewNodes, value);
    }
    
    private List<ICascaderViewItemData> _hoverCascaderNodes = [];
    
    public List<ICascaderViewItemData> HoverCascaderNodes
    {
        get => _hoverCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _hoverCascaderNodes, value);
    }
    
    private List<ICascaderViewItemData> _disabledCascaderNodes = [];
    
    public List<ICascaderViewItemData> DisabledCascaderNodes
    {
        get => _disabledCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _disabledCascaderNodes, value);
    }
    
    private List<ICascaderViewItemData> _selectParentCascaderNodes = [];
    
    public List<ICascaderViewItemData> SelectParentCascaderNodes
    {
        get => _selectParentCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _selectParentCascaderNodes, value);
    }
    
    private List<ICascaderViewItemData> _multipleSelectCascaderNodes = [];
    
    public List<ICascaderViewItemData> MultipleSelectCascaderNodes
    {
        get => _multipleSelectCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _multipleSelectCascaderNodes, value);
    }
    
    private List<ICascaderViewItemData> _checkStrategy1CascaderNodes = [];
    
    public List<ICascaderViewItemData> CheckStrategy1CascaderNodes
    {
        get => _checkStrategy1CascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _checkStrategy1CascaderNodes, value);
    }
    
    private List<ICascaderViewItemData> _checkStrategy2CascaderNodes = [];
    
    public List<ICascaderViewItemData> CheckStrategy2CascaderNodes
    {
        get => _checkStrategy2CascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _checkStrategy2CascaderNodes, value);
    }
    
    private List<ICascaderViewItemData> _lazyLoadCascaderNodes = [];
    
    public List<ICascaderViewItemData> LazyLoadCascaderNodes
    {
        get => _lazyLoadCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _lazyLoadCascaderNodes, value);
    }
    
    private List<ICascaderViewItemData> _prefixAndSuffix1CascaderNodes = [];
    
    public List<ICascaderViewItemData> PrefixAndSuffix1CascaderNodes
    {
        get => _prefixAndSuffix1CascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _prefixAndSuffix1CascaderNodes, value);
    }
    
    private List<ICascaderViewItemData> _prefixAndSuffix2CascaderNodes = [];
    
    public List<ICascaderViewItemData> PrefixAndSuffix2CascaderNodes
    {
        get => _prefixAndSuffix2CascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _prefixAndSuffix2CascaderNodes, value);
    }
    
    private List<ICascaderViewItemData> _prefixAndSuffix3CascaderNodes = [];
    
    public List<ICascaderViewItemData> PrefixAndSuffix3CascaderNodes
    {
        get => _prefixAndSuffix3CascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _prefixAndSuffix3CascaderNodes, value);
    }
    
    private SelectPopupPlacement _placement;

    public SelectPopupPlacement Placement
    {
        get => _placement;
        set => this.RaiseAndSetIfChanged(ref _placement, value);
    }
    
    private List<ICascaderViewItemData> _placementCascaderNodes = [];
    
    public List<ICascaderViewItemData> PlacementCascaderNodes
    {
        get => _placementCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _placementCascaderNodes, value);
    }
    
    private List<ICascaderViewItemData> _searchCascaderNodes = [];
    
    public List<ICascaderViewItemData> SearchCascaderNodes
    {
        get => _searchCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _searchCascaderNodes, value);
    }
    
    private List<ICascaderViewItemData> _sizeLargeCascaderNodes = [];
    
    public List<ICascaderViewItemData> SizeLargeCascaderNodes
    {
        get => _sizeLargeCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _sizeLargeCascaderNodes, value);
    }
 
    private List<ICascaderViewItemData> _sizeMiddleCascaderNodes = [];
    
    public List<ICascaderViewItemData> SizeMiddleCascaderNodes
    {
        get => _sizeMiddleCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _sizeMiddleCascaderNodes, value);
    }
    
    private List<ICascaderViewItemData> _sizeSmallCascaderNodes = [];
    
    public List<ICascaderViewItemData> SizeSmallCascaderNodes
    {
        get => _sizeSmallCascaderNodes;
        set => this.RaiseAndSetIfChanged(ref _sizeSmallCascaderNodes, value);
    }
    
    private List<ICascaderViewItemData> _asyncLoadCascaderViewNodes = [];
    
    public List<ICascaderViewItemData> AsyncLoadCascaderViewNodes
    {
        get => _asyncLoadCascaderViewNodes;
        set => this.RaiseAndSetIfChanged(ref _asyncLoadCascaderViewNodes, value);
    }
    
    private List<ICascaderViewItemData> _searchCascaderViewNodes = [];
    
    public List<ICascaderViewItemData> SearchCascaderViewNodes
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
    
    private List<ICascaderViewItemData> _defaultExpandCascaderViewNodes = [];
    
    public List<ICascaderViewItemData> DefaultExpandCascaderViewNodes
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
    public async Task<CascaderItemLoadResult> LoadAsync(ICascaderViewItemData targetCascaderItem, CancellationToken token)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(600), token);
        var children = new List<CascaderViewItemData>();
        children.AddRange([
            new CascaderViewItemData()
            {
                Header = $"{targetCascaderItem.Value} Dynamic 1",
                IsLeaf = true
            },
            new CascaderViewItemData()
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