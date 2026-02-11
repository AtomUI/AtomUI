using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class TreeSelectViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "TreeSelect";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private List<ITreeItemNode> _basicTreeNodes = [];
    
    public List<ITreeItemNode> BasicTreeNodes
    {
        get => _basicTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _basicTreeNodes, value);
    }
    
    private IList<ITreeItemNode>? _selectedItems;
    
    public IList<ITreeItemNode>? SelectedItems
    {
        get => _selectedItems;
        set => this.RaiseAndSetIfChanged(ref _selectedItems, value);
    }
    
    private List<ITreeItemNode> _multiSelectionTreeNodes = [];
    
    public List<ITreeItemNode> MultiSelectionTreeNodes
    {
        get => _multiSelectionTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _multiSelectionTreeNodes, value);
    }
    
    private List<ITreeItemNode> _itemsSourceTreeNodes = [];
    
    public List<ITreeItemNode> ItemsSourceTreeNodes
    {
        get => _itemsSourceTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _itemsSourceTreeNodes, value);
    }
    
    private List<ITreeItemNode> _checkableTreeNodes = [];
    
    public List<ITreeItemNode> CheckableTreeNodes
    {
        get => _checkableTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _checkableTreeNodes, value);
    }
    
    private List<ITreeItemNode> _asyncLoadTreeNodes = [];
    
    public List<ITreeItemNode> AsyncLoadTreeNodes
    {
        get => _asyncLoadTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _asyncLoadTreeNodes, value);
    }
    
    private TreeItemDataLoader? _asyncLoadTreeNodeLoader;
    
    public TreeItemDataLoader? AsyncLoadTreeNodeLoader
    {
        get => _asyncLoadTreeNodeLoader;
        set => this.RaiseAndSetIfChanged(ref _asyncLoadTreeNodeLoader, value);
    }
    
    private List<ITreeItemNode> _showTreeLineTreeNodes = [];
    
    public List<ITreeItemNode> ShowTreeLineTreeNodes
    {
        get => _showTreeLineTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _showTreeLineTreeNodes, value);
    }
    
    private List<ITreeItemNode> _leftAddTreeNodes = [];
    
    public List<ITreeItemNode> LeftAddTreeNodes
    {
        get => _leftAddTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _leftAddTreeNodes, value);
    }
    
    private List<ITreeItemNode> _contentLeftAddTreeNodes = [];
    
    public List<ITreeItemNode> ContentLeftAddTreeNodes
    {
        get => _contentLeftAddTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _contentLeftAddTreeNodes, value);
    }
    
    private List<ITreeItemNode> _placementTreeNodes = [];
    
    public List<ITreeItemNode> PlacementTreeNodes
    {
        get => _placementTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _placementTreeNodes, value);
    }

    private SelectPopupPlacement _placement;

    public SelectPopupPlacement Placement
    {
        get => _placement;
        set => this.RaiseAndSetIfChanged(ref _placement, value);
    }
    
    private List<ITreeItemNode> _maxSelectedTreeNodes = [];
    
    public List<ITreeItemNode> MaxSelectedTreeNodes
    {
        get => _maxSelectedTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _maxSelectedTreeNodes, value);
    }
    
    private List<ITreeItemNode> _maxCheckedTreeNodes = [];
    
    public List<ITreeItemNode> MaxCheckedTreeNodes
    {
        get => _maxCheckedTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _maxCheckedTreeNodes, value);
    }
    
    public TreeSelectViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}