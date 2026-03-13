using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class TransferViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Transfer";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private List<IListItemData>? _basicTransferItems;
    
    public List<IListItemData>? BasicTransferItems
    {
        get => _basicTransferItems;
        set => this.RaiseAndSetIfChanged(ref _basicTransferItems, value);
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
    
    private TransferFilterValueSelector? _transferFilterValueSelector;
    
    public TransferFilterValueSelector? TransferFilterValueSelector
    {
        get => _transferFilterValueSelector;
        set => this.RaiseAndSetIfChanged(ref _transferFilterValueSelector, value);
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

    public TransferViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}

public record SearchCaseItemData : ListItemData
{
    public string? Description { get; init; }
}