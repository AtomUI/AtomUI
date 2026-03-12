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

    public TransferViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}