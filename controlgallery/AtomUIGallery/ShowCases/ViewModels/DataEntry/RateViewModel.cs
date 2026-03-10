using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class RateViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Rate";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();

    private IList<string>? _tooltips;

    public IList<string>? Tooltips
    {
        get => _tooltips;
        set => this.RaiseAndSetIfChanged(ref _tooltips, value);
    }
    
    private string? _activeTooltip;

    public string? ActiveTooltip
    {
        get => _activeTooltip;
        set => this.RaiseAndSetIfChanged(ref _activeTooltip, value);
    }

    public RateViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}