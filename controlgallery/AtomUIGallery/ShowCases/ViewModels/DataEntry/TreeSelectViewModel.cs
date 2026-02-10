using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class TreeSelectViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "TreeSelect";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private List<ITreeItemData> _basicTreeNodes = [];
    
    public List<ITreeItemData> BasicTreeNodes
    {
        get => _basicTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _basicTreeNodes, value);
    }
    
    private IList<ITreeItemData>? _selectedItems;
    
    public IList<ITreeItemData>? SelectedItems
    {
        get => _selectedItems;
        set => this.RaiseAndSetIfChanged(ref _selectedItems, value);
    }
    
    private List<ITreeItemData> _multiSelectionTreeNodes = [];
    
    public List<ITreeItemData> MultiSelectionTreeNodes
    {
        get => _multiSelectionTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _multiSelectionTreeNodes, value);
    }
    
    private List<ITreeItemData> _itemsSourceTreeNodes = [];
    
    public List<ITreeItemData> ItemsSourceTreeNodes
    {
        get => _itemsSourceTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _itemsSourceTreeNodes, value);
    }
    
    private List<ITreeItemData> _checkableTreeNodes = [];
    
    public List<ITreeItemData> CheckableTreeNodes
    {
        get => _checkableTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _checkableTreeNodes, value);
    }
    
    private List<ITreeItemData> _asyncLoadTreeNodes = [];
    
    public List<ITreeItemData> AsyncLoadTreeNodes
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
    
    private List<ITreeItemData> _showTreeLineTreeNodes = [];
    
    public List<ITreeItemData> ShowTreeLineTreeNodes
    {
        get => _showTreeLineTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _showTreeLineTreeNodes, value);
    }
    
    private List<ITreeItemData> _leftAddTreeNodes = [];
    
    public List<ITreeItemData> LeftAddTreeNodes
    {
        get => _leftAddTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _leftAddTreeNodes, value);
    }
    
    private List<ITreeItemData> _contentLeftAddTreeNodes = [];
    
    public List<ITreeItemData> ContentLeftAddTreeNodes
    {
        get => _contentLeftAddTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _contentLeftAddTreeNodes, value);
    }
    
    private List<ITreeItemData> _placementTreeNodes = [];
    
    public List<ITreeItemData> PlacementTreeNodes
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
    
    private List<ITreeItemData> _maxSelectedTreeNodes = [];
    
    public List<ITreeItemData> MaxSelectedTreeNodes
    {
        get => _maxSelectedTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _maxSelectedTreeNodes, value);
    }
    
    private List<ITreeItemData> _maxCheckedTreeNodes = [];
    
    public List<ITreeItemData> MaxCheckedTreeNodes
    {
        get => _maxCheckedTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _maxCheckedTreeNodes, value);
    }
    
    public TreeSelectViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}