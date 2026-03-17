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
    
    public static readonly StyledProperty<int> PageIndexProperty =
        AvaloniaProperty.Register<ListView, int>(
            nameof(PageIndex), AbstractPagination.DefaultCurrentPage);
    
    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<ListView, int>(
            nameof(PageSize), 0);
    
    public int PageIndex
    {
        get => GetValue(PageIndexProperty);
        set => SetValue(PageIndexProperty, value);
    }
    
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
        }

        if (args.NewValue is AbstractPagination newPagination)
        {
            newPagination.CurrentPageChanged += HandlePageChangeRequest;
            _topPaginationDisposables        =  new CompositeDisposable();
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, TopPaginationAlignProperty, newPagination, AbstractPagination.AlignProperty));
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, IsHideOnSinglePageProperty, newPagination, AbstractPagination.IsHideOnSinglePageProperty));
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, IsEnabledProperty, newPagination, AbstractPagination.IsEnabledProperty));
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, newPagination, AbstractPagination.SizeTypeProperty));
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, newPagination, AbstractPagination.IsMotionEnabledProperty));
            _topPaginationDisposables.Add(BindUtils.RelayBind(this, PaginationVisibilityProperty, newPagination, AbstractPagination.IsMotionEnabledProperty));
            _topPagination = newPagination;
            HandlePaginationVisibility();
        }
    }
    
    private void HandleBottomPaginationChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is AbstractPagination oldPagination)
        {
            oldPagination.CurrentPageChanged -= HandlePageChangeRequest;
            _bottomPaginationDisposables?.Dispose();
        }

        if (args.NewValue is AbstractPagination newPagination)
        {
            newPagination.CurrentPageChanged += HandlePageChangeRequest;
            _bottomPaginationDisposables     =  new CompositeDisposable();
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, TopPaginationAlignProperty, newPagination, AbstractPagination.AlignProperty));
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, IsHideOnSinglePageProperty, newPagination, AbstractPagination.IsHideOnSinglePageProperty));
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, IsEnabledProperty, newPagination, AbstractPagination.IsEnabledProperty));
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, SizeTypeProperty, newPagination, AbstractPagination.SizeTypeProperty));
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, newPagination, AbstractPagination.IsMotionEnabledProperty));
            _bottomPaginationDisposables.Add(BindUtils.RelayBind(this, PaginationVisibilityProperty, newPagination, AbstractPagination.IsMotionEnabledProperty));
            _bottomPagination = newPagination;
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
        }
    }
    
    private void ReConfigurePagination()
    {
        if (_collectionView != null)
        {
            _collectionView.PageSize = PageSize;

            if (_topPagination != null)
            {
                _topPagination.Total       = _collectionView.TotalItemCount;
                _topPagination.PageSize    = PageSize;
                _topPagination.CurrentPage = PageIndex + 1;
            }

            if (_bottomPagination != null)
            {
                _bottomPagination.Total       = _collectionView.TotalItemCount;
                _bottomPagination.PageSize    = PageSize;
                _bottomPagination.CurrentPage = PageIndex + 1;
            }
        }
        
    }
}