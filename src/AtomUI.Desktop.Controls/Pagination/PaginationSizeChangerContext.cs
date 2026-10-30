using Avalonia;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Provides the effective page-size state to a custom <see cref="Pagination.SizeChangerTemplate"/>.
/// </summary>
public sealed class PaginationSizeChangerContext : AvaloniaObject
{
    #region 公共属性定义

    public static readonly DirectProperty<PaginationSizeChangerContext, int> PageSizeProperty =
        AvaloniaProperty.RegisterDirect<PaginationSizeChangerContext, int>(
            nameof(PageSize),
            context => context.PageSize,
            (context, value) => context.PageSize = value,
            AbstractPagination.DefaultPageSize,
            BindingMode.TwoWay);

    public static readonly DirectProperty<PaginationSizeChangerContext, CustomizableSizeType> SizeTypeProperty =
        AvaloniaProperty.RegisterDirect<PaginationSizeChangerContext, CustomizableSizeType>(
            nameof(SizeType),
            context => context.SizeType);

    private int _pageSize = AbstractPagination.DefaultPageSize;

    /// <summary>
    /// Gets or requests the effective number of items displayed on each page.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => RequestPageSizeChange(value);
    }

    private CustomizableSizeType _sizeType;

    /// <summary>
    /// Gets the size variant inherited from the owning <see cref="Pagination"/>.
    /// </summary>
    public CustomizableSizeType SizeType => _sizeType;

    #endregion

    private readonly WeakReference<Pagination> _ownerReference;

    internal PaginationSizeChangerContext(Pagination owner)
    {
        _ownerReference = new WeakReference<Pagination>(owner);
    }

    internal void Synchronize(int pageSize, CustomizableSizeType sizeType)
    {
        var effectivePageSize = pageSize <= 0 ? AbstractPagination.DefaultPageSize : pageSize;
        if (_pageSize != effectivePageSize)
        {
            SetAndRaise(PageSizeProperty, ref _pageSize, effectivePageSize);
        }

        if (_sizeType != sizeType)
        {
            SetAndRaise(SizeTypeProperty, ref _sizeType, sizeType);
        }
    }

    private void RequestPageSizeChange(int pageSize)
    {
        if (pageSize <= 0)
        {
            return;
        }

        if (_ownerReference.TryGetTarget(out var owner) && owner.PageSize != pageSize)
        {
            owner.SetCurrentValue(AbstractPagination.PageSizeProperty, pageSize);
        }
    }
}
