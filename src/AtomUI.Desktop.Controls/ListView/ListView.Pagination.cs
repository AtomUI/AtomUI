using System.ComponentModel;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Data;
using AtomUI.Data;
using Avalonia;

namespace AtomUI.Desktop.Controls;

public partial class ListView
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="PageIndex" /> property.
    /// </summary>
    public static readonly StyledProperty<int> PageIndexProperty =
        AvaloniaProperty.Register<ListView, int>(
            nameof(PageIndex), AbstractPagination.DefaultCurrentPage);
    
    /// <summary>
    /// Identifies the <see cref="PageSize" /> property.
    /// </summary>
    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<ListView, int>(
            nameof(PageSize), 0);
    
    /// <summary>
    /// Zero-based index of the page shown by the data view, as reported by the last page change of the view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The list updates this value when the data view reports a page change. Until the first report it holds
    /// the default <see cref="AbstractPagination.DefaultCurrentPage" />, which is a one-based pager page number.
    /// </para>
    /// <para>
    /// Assigning a value is not applied to the data view. Change pages through the pagers, whose page number
    /// is one-based.
    /// </para>
    /// </remarks>
    public int PageIndex
    {
        get => GetValue(PageIndexProperty);
        set => SetValue(PageIndexProperty, value);
    }
    
    /// <summary>
    /// Number of items per page. The default is <c>0</c>, which turns paging off; a value greater than zero
    /// turns paging on and is handed to both pagers as well.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The value is passed to the data view whenever it changes. When the data view changes its page size on
    /// its own, this property follows.
    /// </para>
    /// </remarks>
    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }
    
    #endregion
    
    private AbstractPagination? _topPagination;
    private AbstractPagination? _bottomPagination;
    
    private CompositeDisposable? _topPaginationDisposables;
    private CompositeDisposable? _bottomPaginationDisposables;

    private void HandlePropertyChangedForPagination(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == PageSizeProperty)
        {
            if (_collectionView != null)
            {
                _collectionView.PageSize = PageSize;
                SyncPaginationState();
            }
        }
        else if (change.Property == TopPaginationProperty)
        {
            HandleTopPaginationChanged(change);
        }
        else if (change.Property == BottomPaginationProperty)
        {
            HandleBottomPaginationChanged(change);
        }
        else if (change.Property == PaginationVisibilityProperty)
        {
            HandlePaginationVisibility();
        }
    }
    
    private void HandleTopPaginationChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is AbstractPagination oldPagination)
        {
            oldPagination.CurrentPageChanged -= HandlePageChangeRequest;
            _topPaginationDisposables?.Dispose();
            _topPaginationDisposables = null;
            if (ReferenceEquals(_topPagination, oldPagination))
            {
                _topPagination = null;
            }
        }

        if (args.NewValue is AbstractPagination newPagination)
        {
            newPagination.CurrentPageChanged += HandlePageChangeRequest;
            _topPaginationDisposables        =  new CompositeDisposable();
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, TopPaginationAlignProperty, newPagination, AbstractPagination.AlignProperty));
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, IsHideOnSinglePageProperty, newPagination, AbstractPagination.IsHideOnSinglePageProperty));
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, IsEnabledProperty, newPagination, AbstractPagination.IsEnabledProperty));
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, newPagination, AbstractPagination.IsMotionEnabledProperty));
            _topPagination = newPagination;
            SyncPaginationState();
            HandlePaginationVisibility();
        }
    }
    
    private void HandleBottomPaginationChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is AbstractPagination oldPagination)
        {
            oldPagination.CurrentPageChanged -= HandlePageChangeRequest;
            _bottomPaginationDisposables?.Dispose();
            _bottomPaginationDisposables = null;
            if (ReferenceEquals(_bottomPagination, oldPagination))
            {
                _bottomPagination = null;
            }
        }

        if (args.NewValue is AbstractPagination newPagination)
        {
            newPagination.CurrentPageChanged += HandlePageChangeRequest;
            _bottomPaginationDisposables     =  new CompositeDisposable();
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, BottomPaginationAlignProperty, newPagination, AbstractPagination.AlignProperty));
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, IsHideOnSinglePageProperty, newPagination, AbstractPagination.IsHideOnSinglePageProperty));
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, IsEnabledProperty, newPagination, AbstractPagination.IsEnabledProperty));
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, newPagination, AbstractPagination.IsMotionEnabledProperty));
            _bottomPagination = newPagination;
            SyncPaginationState();
            HandlePaginationVisibility();
        }
    }

    private void HandlePaginationVisibility()
    {
        if (PaginationVisibility == ListPaginationVisibility.None)
        {
            _topPagination?.IsVisible    = false;
            _bottomPagination?.IsVisible = false;
        }
        else if (PaginationVisibility == ListPaginationVisibility.Both)
        {
            _topPagination?.IsVisible    = true;
            _bottomPagination?.IsVisible = true;
        }
        else if (PaginationVisibility == ListPaginationVisibility.Top)
        {
            _topPagination?.IsVisible    = true;
            _bottomPagination?.IsVisible = false;
        }
        else
        {
            _topPagination?.IsVisible    = false;
            _bottomPagination?.IsVisible = true;
        }
    }
    
    private void HandlePageChangeRequest(object? sender, PageChangedEventArgs args)
    {
        if (_collectionView != null)
        {
            _collectionView.MoveToPage(args.PageIndex - 1);
        }
    }

    private void HandleCollectionPropertyChanged(object? sender, PropertyChangedEventArgs change)
    {
        if (sender is IListCollectionView listCollectionView)
        {
            if (change.PropertyName == nameof(ListCollectionView.PageSize))
            {
                SetCurrentValue(PageSizeProperty, listCollectionView.PageSize);
                SyncPaginationState();
            }
        }
    }
    
    private void HandlePageChanging(object? sender, PageChangingEventArgs args)
    {
    }

    private void HandlePageChanged(object? sender, EventArgs args)
    {
        if (_collectionView != null)
        {
            SetCurrentValue(PageIndexProperty, _collectionView.PageIndex);
            SyncPaginationState();
        }
    }
    
    private void ReConfigurePagination()
    {
        if (_collectionView != null)
        {
            _collectionView.PageSize = PageSize;
            SyncPaginationState();
        }
    }

    private void SyncPaginationState()
    {
        if (_collectionView == null)
        {
            return;
        }

        SyncPaginationState(_topPagination);
        SyncPaginationState(_bottomPagination);
    }

    private void SyncPaginationState(AbstractPagination? pagination)
    {
        if (pagination == null || _collectionView == null)
        {
            return;
        }

        pagination.Total       = _collectionView.TotalItemCount;
        pagination.PageSize    = PageSize;
        pagination.CurrentPage = _collectionView.PageIndex < 0
            ? AbstractPagination.DefaultCurrentPage
            : _collectionView.PageIndex + 1;
    }
}
