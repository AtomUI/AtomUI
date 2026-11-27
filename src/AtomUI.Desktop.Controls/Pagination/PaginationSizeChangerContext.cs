using Avalonia;
using Avalonia.Data;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Provides the effective page-size state to a custom <see cref="Pagination.SizeChangerTemplate" />.
/// </summary>
public sealed class PaginationSizeChangerContext : AvaloniaObject
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="PageSize" /> property.
    /// </summary>
    public static readonly DirectProperty<PaginationSizeChangerContext, int> PageSizeProperty =
        AvaloniaProperty.RegisterDirect<PaginationSizeChangerContext, int>(
            nameof(PageSize),
            context => context.PageSize,
            (context, value) => context.PageSize = value,
            AbstractPagination.DefaultPageSize,
            BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly DirectProperty<PaginationSizeChangerContext, CustomizableSizeType> SizeTypeProperty =
        AvaloniaProperty.RegisterDirect<PaginationSizeChangerContext, CustomizableSizeType>(
            nameof(SizeType),
            context => context.SizeType);

    private int _pageSize = AbstractPagination.DefaultPageSize;

    /// <summary>
    /// Gets the number of items on a page, or requests a new one by setting it. Setting a value greater than zero
    /// asks the owning <see cref="Pagination" /> to change its page size; the context shows the new value once the
    /// owner has synchronized it. A value of zero or less is ignored.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => RequestPageSizeChange(value);
    }

    private CustomizableSizeType _sizeType;

    /// <summary>
    /// Gets the size variant inherited from the owning <see cref="Pagination" />.
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
