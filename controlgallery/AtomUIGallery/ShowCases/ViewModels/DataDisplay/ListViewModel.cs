using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using Avalonia.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class ListViewModel : ReactiveObject, IRoutableViewModel
{
    public static TreeNodeKey ID = "List";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private List<IListItemData>? _listItems;
    
    public List<IListItemData>? ListItems
    {
        get => _listItems;
        set => this.RaiseAndSetIfChanged(ref _listItems, value);
    }
    
    private List<IListItemData>? _listItemsWidthDisabled = [];
    
    public List<IListItemData>? ListItemsWidthDisabled
    {
        get => _listItemsWidthDisabled;
        set => this.RaiseAndSetIfChanged(ref _listItemsWidthDisabled, value);
    }
    
    private List<IListItemData>? _groupListItems;
    
    public List<IListItemData>? GroupListItems
    {
        get => _groupListItems;
        set => this.RaiseAndSetIfChanged(ref _groupListItems, value);
    }
    
    private List<IListItemData>? _emptyDemoItems;

    public List<IListItemData>? EmptyDemoItems
    {
        get => _emptyDemoItems;
        set => this.RaiseAndSetIfChanged(ref _emptyDemoItems, value);
    }

    private SelectionMode _selectionMode;
    
    public SelectionMode SelectionMode
    {
        get => _selectionMode;
        set => this.RaiseAndSetIfChanged(ref _selectionMode, value);
    }
    
    private List<IListItemData>? _basicListBoxItems;
    
    public List<IListItemData>? BasicListBoxItems
    {
        get => _basicListBoxItems;
        set => this.RaiseAndSetIfChanged(ref _basicListBoxItems, value);
    }
    
    public ListViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}
