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