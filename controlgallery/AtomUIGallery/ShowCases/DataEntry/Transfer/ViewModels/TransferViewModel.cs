using System.Collections.ObjectModel;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Controls.Utils;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Transfer;

public class TransferViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Transfer";

    public IScreen HostScreen { get; }

    public string? UrlPathSegment => ID.ToString();

    private List<IListItemData>? _basicTransferItems;

    public List<IListItemData>? BasicTransferItems
    {
        get => _basicTransferItems;
        set => this.RaiseAndSetIfChanged(ref _basicTransferItems, value);
    }

    private List<EntityKey>? _semanticDemoTargetKeys;

    public List<EntityKey>? SemanticDemoTargetKeys
    {
        get => _semanticDemoTargetKeys;
        set => this.RaiseAndSetIfChanged(ref _semanticDemoTargetKeys, value);
    }

    private List<EntityKey>? _semanticPreviewTargetKeys;

    public List<EntityKey>? SemanticPreviewTargetKeys
    {
        get => _semanticPreviewTargetKeys;
        set => this.RaiseAndSetIfChanged(ref _semanticPreviewTargetKeys, value);
    }


    private List<EntityKey>? _semanticTreePreviewTargetKeys;

    public List<EntityKey>? SemanticTreePreviewTargetKeys
    {
        get => _semanticTreePreviewTargetKeys;
        set => this.RaiseAndSetIfChanged(ref _semanticTreePreviewTargetKeys, value);
    }

    private List<IListItemData>? _oneWayTransferItems;

    public List<IListItemData>? OneWayTransferItems
    {
        get => _oneWayTransferItems;
        set => this.RaiseAndSetIfChanged(ref _oneWayTransferItems, value);
    }

    private bool _oneWayTransferEnabled = true;

    public bool OneWayTransferEnabled
    {
        get => _oneWayTransferEnabled;
        set => this.RaiseAndSetIfChanged(ref _oneWayTransferEnabled, value);
    }

    private List<IListItemData>? _searchTransferItems;

    public List<IListItemData>? SearchTransferItems
    {
        get => _searchTransferItems;
        set => this.RaiseAndSetIfChanged(ref _searchTransferItems, value);
    }

    private DefaultFilterValueSelector? _transferFilterValueSelector;

    public DefaultFilterValueSelector? TransferFilterValueSelector
    {
        get => _transferFilterValueSelector;
        set => this.RaiseAndSetIfChanged(ref _transferFilterValueSelector, value);
    }

    private List<IListItemData>? _controlledTransferItems;

    public List<IListItemData>? ControlledTransferItems
    {
        get => _controlledTransferItems;
        set => this.RaiseAndSetIfChanged(ref _controlledTransferItems, value);
    }

    private ObservableCollection<EntityKey>? _controlledTransferTargetKeys;

    public ObservableCollection<EntityKey>? ControlledTransferTargetKeys
    {
        get => _controlledTransferTargetKeys;
        set => this.RaiseAndSetIfChanged(ref _controlledTransferTargetKeys, value);
    }

    private ObservableCollection<EntityKey>? _controlledTransferSelectedKeys;

    public ObservableCollection<EntityKey>? ControlledTransferSelectedKeys
    {
        get => _controlledTransferSelectedKeys;
        set => this.RaiseAndSetIfChanged(ref _controlledTransferSelectedKeys, value);
    }

    private List<IListItemData>? _advanceTransferItems;

    public List<IListItemData>? AdvanceTransferItems
    {
        get => _advanceTransferItems;
        set => this.RaiseAndSetIfChanged(ref _advanceTransferItems, value);
    }

    private List<EntityKey>? _advanceTransferDefaultTargetKeys;

    public List<EntityKey>? AdvanceTransferDefaultTargetKeys
    {
        get => _advanceTransferDefaultTargetKeys;
        set => this.RaiseAndSetIfChanged(ref _advanceTransferDefaultTargetKeys, value);
    }

    private bool _paginationIsOneWay = false;

    public bool PaginationIsOneWay
    {
        get => _paginationIsOneWay;
        set => this.RaiseAndSetIfChanged(ref _paginationIsOneWay, value);
    }

    private List<IListItemData>? _paginationTransferItems;

    public List<IListItemData>? PaginationTransferItems
    {
        get => _paginationTransferItems;
        set => this.RaiseAndSetIfChanged(ref _paginationTransferItems, value);
    }

    private List<EntityKey>? _paginationTransferDefaultTargetKeys;

    public List<EntityKey>? PaginationTransferDefaultTargetKeys
    {
        get => _paginationTransferDefaultTargetKeys;
        set => this.RaiseAndSetIfChanged(ref _paginationTransferDefaultTargetKeys, value);
    }

    private List<DataGridTransferData>? _gridDataTransformItems;

    public List<DataGridTransferData>? GridDataTransformItems
    {
        get => _gridDataTransformItems;
        set => this.RaiseAndSetIfChanged(ref _gridDataTransformItems, value);
    }

    private List<ITreeItemNode>? _transferTreeNodes = [];

    public List<ITreeItemNode>? TransferTreeNodes
    {
        get => _transferTreeNodes;
        set => this.RaiseAndSetIfChanged(ref _transferTreeNodes, value);
    }

    private bool _treeTransferIsOneWay = false;

    public bool TreeTransferIsOneWay
    {
        get => _treeTransferIsOneWay;
        set => this.RaiseAndSetIfChanged(ref _treeTransferIsOneWay, value);
    }

    public TransferViewModel(IScreen screen)
    {
        HostScreen = screen;
    }

}

public class SearchCaseItemData : ListItemData
{
    public string? Description { get; init; }
}

public record DataGridTransferData : IItemKey
{
    public EntityKey? ItemKey { get; init; }
    public string? Title { get; init; }
    public string? Description  { get; init; }
    public string? Tag  { get; init; }
}
