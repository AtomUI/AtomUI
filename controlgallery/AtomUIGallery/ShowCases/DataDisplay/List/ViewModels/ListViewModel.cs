using System.Reactive;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Desktop.Controls;
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

    private string? _selectionExampleText;

    public string SelectionExampleText =>
        _selectionExampleText ?? ListShowCase.Lang(ListShowCaseLangResourceKind.P2TextNoSelection, "No selection");

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

    public ReactiveCommand<IListViewSelection, Unit> SelectExampleItemsCommand { get; }

    public ReactiveCommand<IListViewSelection, Unit> ClearExampleSelectionCommand { get; }

    public ListViewModel(IScreen screen)
    {
        HostScreen                   = screen;
        SelectExampleItemsCommand    = ReactiveCommand.Create<IListViewSelection>(SelectExampleItems);
        ClearExampleSelectionCommand = ReactiveCommand.Create<IListViewSelection>(selection => selection.Clear());
    }

    internal void UpdateSelectionExampleText(IReadOnlyList<object?> selectedItems)
    {
        var text = selectedItems.Count == 0
            ? ListShowCase.Lang(ListShowCaseLangResourceKind.P2TextNoSelection, "No selection")
            : string.Join(", ", selectedItems.OfType<IListItemData>()
                                            .Select(item => item.Content?.ToString())
                                            .Where(content => !string.IsNullOrWhiteSpace(content)));
        this.RaiseAndSetIfChanged(ref _selectionExampleText, text, nameof(SelectionExampleText));
    }

    private void SelectExampleItems(IListViewSelection selection)
    {
        selection.Clear();
        if (SelectionListItems is { Count: > 3 })
        {
            selection.Select(1);
            selection.Select(3);
        }
    }
}
