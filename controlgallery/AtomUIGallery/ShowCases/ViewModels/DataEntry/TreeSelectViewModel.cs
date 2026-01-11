using AtomUI;
using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class TreeSelectViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "TreeSelect";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private List<ITreeViewItemData> _basicTreeNodes = [];
    
    public List<ITreeViewItemData> BasicTreeNodes
    {
        get => _basicTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _basicTreeNodes, value);
    }
    
    private List<ITreeViewItemData> _multiSelectionTreeNodes = [];
    
    public List<ITreeViewItemData> MultiSelectionTreeNodes
    {
        get => _multiSelectionTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _multiSelectionTreeNodes, value);
    }
    
    private List<ITreeViewItemData> _itemsSourceTreeNodes = [];
    
    public List<ITreeViewItemData> ItemsSourceTreeNodes
    {
        get => _itemsSourceTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _itemsSourceTreeNodes, value);
    }
    
    private List<ITreeViewItemData> _checkableTreeNodes = [];
    
    public List<ITreeViewItemData> CheckableTreeNodes
    {
        get => _checkableTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _checkableTreeNodes, value);
    }
    
    private List<ITreeViewItemData> _asyncLoadTreeNodes = [];
    
    public List<ITreeViewItemData> AsyncLoadTreeNodes
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
    
    private List<ITreeViewItemData> _showTreeLineTreeNodes = [];
    
    public List<ITreeViewItemData> ShowTreeLineTreeNodes
    {
        get => _showTreeLineTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _showTreeLineTreeNodes, value);
    }
    
    private List<ITreeViewItemData> _leftAddTreeNodes = [];
    
    public List<ITreeViewItemData> LeftAddTreeNodes
    {
        get => _leftAddTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _leftAddTreeNodes, value);
    }
    
    private List<ITreeViewItemData> _contentLeftAddTreeNodes = [];
    
    public List<ITreeViewItemData> ContentLeftAddTreeNodes
    {
        get => _contentLeftAddTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _contentLeftAddTreeNodes, value);
    }
    
    private List<ITreeViewItemData> _placementTreeNodes = [];
    
    public List<ITreeViewItemData> PlacementTreeNodes
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
    
    public TreeSelectViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}