using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Desktop.Controls.Primitives;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class TreeViewViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "TreeView";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private bool _showLineSwitchChecked = true;

    public bool ShowLineSwitchChecked
    {
        get => _showLineSwitchChecked;
        set => this.RaiseAndSetIfChanged(ref _showLineSwitchChecked, value);
    }
    
    private bool _showIconSwitchChecked;

    public bool ShowIconSwitchChecked
    {
        get => _showIconSwitchChecked;
        set => this.RaiseAndSetIfChanged(ref _showIconSwitchChecked, value);
    }

    private bool _showLeafIconSwitchChecked;

    public bool ShowLeafIconSwitchChecked
    {
        get => _showLeafIconSwitchChecked;
        set => this.RaiseAndSetIfChanged(ref _showLeafIconSwitchChecked, value);
    }
    
    private IList<TreeNodePath>? _basicTreeViewDefaultExpandedPaths;

    public IList<TreeNodePath>? BasicTreeViewDefaultExpandedPaths
    {
        get => _basicTreeViewDefaultExpandedPaths;
        set => this.RaiseAndSetIfChanged(ref _basicTreeViewDefaultExpandedPaths, value);
    }
    
    private IList<TreeNodePath>? _customizeCollapseExpandTreeDefaultExpandedPaths;

    public IList<TreeNodePath>? CustomizeCollapseExpandTreeDefaultExpandedPaths
    {
        get => _customizeCollapseExpandTreeDefaultExpandedPaths;
        set => this.RaiseAndSetIfChanged(ref _customizeCollapseExpandTreeDefaultExpandedPaths, value);
    }
    
    private IList<TreeNodePath>? _basicTreeViewDefaultSelectedPaths;

    public IList<TreeNodePath>? BasicTreeViewDefaultSelectedPaths
    {
        get => _basicTreeViewDefaultSelectedPaths;
        set => this.RaiseAndSetIfChanged(ref _basicTreeViewDefaultSelectedPaths, value);
    }
    
    private IList<TreeNodePath>? _basicTreeViewDefaultCheckedPaths;

    public IList<TreeNodePath>? BasicTreeViewDefaultCheckedPaths
    {
        get => _basicTreeViewDefaultCheckedPaths;
        set => this.RaiseAndSetIfChanged(ref _basicTreeViewDefaultCheckedPaths, value);
    }
    
    private TreeItemHoverMode _treeViewNodeHoverMode;

    public TreeItemHoverMode TreeViewNodeHoverMode
    {
        get => _treeViewNodeHoverMode;
        set => this.RaiseAndSetIfChanged(ref _treeViewNodeHoverMode, value);
    }
    
    private List<ITreeViewItemData> _basicTreeNodes = [];
    
    public List<ITreeViewItemData> BasicTreeNodes
    {
        get => _basicTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _basicTreeNodes, value);
    }

    public TreeViewViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}