using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class TreeSelectViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "TreeSelect";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private List<ITreeViewItemData> _basicTreeSelectNodes = [];
    
    public List<ITreeViewItemData> BasicTreeSelectNodes
    {
        get => _basicTreeSelectNodes;
        set => this.RaiseAndSetIfChanged(ref _basicTreeSelectNodes, value);
    }
    
    private List<ITreeViewItemData> _multiSelectionTreeSelectNodes = [];
    
    public List<ITreeViewItemData> MultiSelectionTreeSelectNodes
    {
        get => _multiSelectionTreeSelectNodes;
        set => this.RaiseAndSetIfChanged(ref _multiSelectionTreeSelectNodes, value);
    }
    
    private List<ITreeViewItemData> _itemsSourceTreeSelectNodes = [];
    
    public List<ITreeViewItemData> ItemsSourceTreeSelectNodes
    {
        get => _itemsSourceTreeSelectNodes;
        set => this.RaiseAndSetIfChanged(ref _itemsSourceTreeSelectNodes, value);
    }
    
    private List<ITreeViewItemData> _checkableTreeSelectNodes = [];
    
    public List<ITreeViewItemData> CheckableTreeSelectNodes
    {
        get => _checkableTreeSelectNodes;
        set => this.RaiseAndSetIfChanged(ref _checkableTreeSelectNodes, value);
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
    
    public TreeSelectViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}