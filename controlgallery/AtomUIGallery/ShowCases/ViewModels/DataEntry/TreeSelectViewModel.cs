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

    public TreeSelectViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}