using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.ViewModels;

public class CarouselViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Carousel";
    
    public IScreen HostScreen { get; }
    
    public string UrlPathSegment { get; } = ID.ToString();
    
    private CarouselPaginationPosition _paginationPosition = CarouselPaginationPosition.Bottom;

    public CarouselPaginationPosition PaginationPosition
    {
        get => _paginationPosition;
        set => this.RaiseAndSetIfChanged(ref _paginationPosition, value);
    }
    
    public CarouselViewModel(IScreen screen)
    {
        HostScreen = screen;
    }
}