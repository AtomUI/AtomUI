using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.List;

public class ListViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "List";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private List<IListItemData>? _listItems;

    public List<IListItemData>? ListItems
    {
        get => _listItems;
        set => this.RaiseAndSetIfChanged(ref _listItems, value);
    }

    private List<IListItemData>? _selectionListItems;

    public List<IListItemData>? SelectionListItems
    {
        get => _selectionListItems;
        set => this.RaiseAndSetIfChanged(ref _selectionListItems, value);
    }

    private ObservableCollection<IListItemData> _boundSelectedItems = [];

    public ObservableCollection<IListItemData> BoundSelectedItems
    {
        get => _boundSelectedItems;
        set
        {
            var nextSelectedItems = value ?? [];

            if (ReferenceEquals(_boundSelectedItems, nextSelectedItems))
            {
                return;
            }

            _boundSelectedItems.CollectionChanged -= HandleBoundSelectedItemsCollectionChanged;
            this.RaiseAndSetIfChanged(ref _boundSelectedItems, nextSelectedItems);
            _boundSelectedItems.CollectionChanged += HandleBoundSelectedItemsCollectionChanged;
            this.RaisePropertyChanged(nameof(BoundSelectedItemsText));
        }
    }

    public string BoundSelectedItemsText
    {
        get
        {
            if (BoundSelectedItems.Count == 0)
            {
                return Lang(ListShowCaseLangResourceKind.P2TextNoSelection);
            }

            return string.Join(
                ", ",
                BoundSelectedItems
                    .Select(item => item.Content?.ToString())
                    .Where(text => !string.IsNullOrWhiteSpace(text)));
        }
    }

    private List<IListItemData>? _listItemsWidthDisabled = [];

    public List<IListItemData>? ListItemsWidthDisabled
    {
        get => _listItemsWidthDisabled;
        set => this.RaiseAndSetIfChanged(ref _listItemsWidthDisabled, value);
    }

    private List<IListItemData>? _filteredGroupListItems;

    public List<IListItemData>? FilteredGroupListItems
    {
        get => _filteredGroupListItems;
        set => this.RaiseAndSetIfChanged(ref _filteredGroupListItems, value);
    }

    private List<IListItemData>? _orderedGroupListItems;

    public List<IListItemData>? OrderedGroupListItems
    {
        get => _orderedGroupListItems;
        set => this.RaiseAndSetIfChanged(ref _orderedGroupListItems, value);
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
    private string? _searchFilterValue;
    private IList<IListSortDescription>? _orderedSortDescriptions;

    public SelectionMode SelectionMode
    {
        get => _selectionMode;
        set => this.RaiseAndSetIfChanged(ref _selectionMode, value);
    }

    public string? SearchFilterValue
    {
        get => _searchFilterValue;
        set => this.RaiseAndSetIfChanged(ref _searchFilterValue, value);
    }

    public IList<IListSortDescription>? OrderedSortDescriptions
    {
        get => _orderedSortDescriptions;
        set => this.RaiseAndSetIfChanged(ref _orderedSortDescriptions, value);
    }

    private List<IListItemData>? _basicListBoxItems;

    public List<IListItemData>? BasicListBoxItems
    {
        get => _basicListBoxItems;
        set => this.RaiseAndSetIfChanged(ref _basicListBoxItems, value);
    }

    private List<IListItemData>? _paginationListItems;

    public List<IListItemData>? PaginationListItems
    {
        get => _paginationListItems;
        set => this.RaiseAndSetIfChanged(ref _paginationListItems, value);
    }

    private List<IListItemData>? _semanticListItems;

    public List<IListItemData>? SemanticListItems
    {
        get => _semanticListItems;
        set => this.RaiseAndSetIfChanged(ref _semanticListItems, value);
    }

    private List<IListItemData>? _semanticListBoxItems;

    public List<IListItemData>? SemanticListBoxItems
    {
        get => _semanticListBoxItems;
        set => this.RaiseAndSetIfChanged(ref _semanticListBoxItems, value);
    }

    public ReactiveCommand<Unit, Unit> SelectBoundSelectedItemsCommand { get; }

    public ReactiveCommand<Unit, Unit> ClearBoundSelectedItemsCommand { get; }

    public ListViewModel(IScreen screen)
    {
        HostScreen                         = screen;
        SelectBoundSelectedItemsCommand    = ReactiveCommand.Create(SelectBoundSelectedItems);
        ClearBoundSelectedItemsCommand     = ReactiveCommand.Create(ClearBoundSelectedItems);
        _boundSelectedItems.CollectionChanged += HandleBoundSelectedItemsCollectionChanged;
    }

    public void ResetBoundSelectedItems()
    {
        if (SelectionListItems is { Count: > 1 })
        {
            BoundSelectedItems = new ObservableCollection<IListItemData>
            {
                SelectionListItems[1]
            };
            return;
        }

        BoundSelectedItems = [];
    }

    public void ClearBoundSelectedItems()
    {
        BoundSelectedItems.Clear();
    }

    private void SelectBoundSelectedItems()
    {
        BoundSelectedItems.Clear();

        if (SelectionListItems is not { Count: > 3 })
        {
            return;
        }

        BoundSelectedItems.Add(SelectionListItems[1]);
        BoundSelectedItems.Add(SelectionListItems[3]);
    }

    private void HandleBoundSelectedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(BoundSelectedItemsText));
    }

    private static string Lang(ListShowCaseLangResourceKind kind)
    {
        return Application.Current is { } application
            ? global::AtomUI.ApplicationExtensions.GetLocalizer(application)?.Get(kind) ?? kind.ToString()
            : kind.ToString();
    }
}
