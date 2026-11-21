using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Card;

public class CardViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Card";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    public CardViewModel(IScreen screen)
    {
        HostScreen = screen;
    }

}
