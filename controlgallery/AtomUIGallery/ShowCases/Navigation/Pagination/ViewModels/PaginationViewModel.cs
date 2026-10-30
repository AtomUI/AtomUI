using System.Reactive;
using AtomUI.Controls;
using ReactiveUI;

namespace AtomUIGallery.ShowCases.Pagination;

public class PaginationViewModel : ReactiveObject, IRoutableViewModel
{
    public static EntityKey ID = "Pagination";

    public IScreen HostScreen { get; }

    public string UrlPathSegment { get; } = ID.ToString();

    private int _boundCurrentPage = 2;
    private int _boundPageSize = 10;

    public int BoundCurrentPage
    {
        get => _boundCurrentPage;
        set
        {
            if (_boundCurrentPage == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _boundCurrentPage, value);
            this.RaisePropertyChanged(nameof(BoundCurrentPageText));
        }
    }

    public string BoundCurrentPageText => BoundCurrentPage.ToString(GalleryLocalization.GetFormattingCulture());

    public int BoundPageSize
    {
        get => _boundPageSize;
        set
        {
            if (_boundPageSize == value)
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref _boundPageSize, value);
            this.RaisePropertyChanged(nameof(BoundPageSizeText));
        }
    }

    public string BoundPageSizeText => BoundPageSize.ToString(GalleryLocalization.GetFormattingCulture());

    public ReactiveCommand<Unit, Unit> SetBoundPaginationCommand { get; }

    public ReactiveCommand<Unit, Unit> ResetBoundPaginationCommand { get; }

    public PaginationViewModel(IScreen screen)
    {
        HostScreen                     = screen;
        SetBoundPaginationCommand      = ReactiveCommand.Create(SetBoundPagination);
        ResetBoundPaginationCommand    = ReactiveCommand.Create(ResetBoundPagination);
    }

    private void SetBoundPagination()
    {
        BoundCurrentPage = 5;
        BoundPageSize    = 20;
    }

    private void ResetBoundPagination()
    {
        BoundCurrentPage = 2;
        BoundPageSize    = 10;
    }

}
