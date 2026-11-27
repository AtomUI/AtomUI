using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Base class of the pagers. It holds the page condition (<see cref="Total" />, <see cref="PageSize" />
/// and <see cref="CurrentPage" />), derives <see cref="PageCount" /> from it, decides whether the pager
/// content is shown, and raises <see cref="CurrentPageChanged" /> when the condition was recalculated.
/// A derived pager builds and updates its own items from these values.
/// </summary>
public abstract class AbstractPagination : TemplatedControl, ICustomizableSizeTypeAware, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Page size used to calculate <see cref="PageCount" /> while <see cref="PageSize" /> is 0.
    /// </summary>
    public const int DefaultPageSize = 10;
    /// <summary>
    /// Number of the first page; page numbers start at 1.
    /// </summary>
    public const int DefaultCurrentPage = 1;

    /// <summary>
    /// Identifies the <see cref="IsHideOnSinglePage" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsHideOnSinglePageProperty =
        AvaloniaProperty.Register<AbstractPagination, bool>(nameof(IsHideOnSinglePage));
    
    /// <summary>
    /// Identifies the <see cref="Align" /> property.
    /// </summary>
    public static readonly StyledProperty<PaginationAlign> AlignProperty =
        AvaloniaProperty.Register<AbstractPagination, PaginationAlign>(nameof(Align));
    
    /// <summary>
    /// Identifies the <see cref="BorderDashArray" /> property.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<double>?> BorderDashArrayProperty =
        AvaloniaProperty.Register<AbstractPagination, IReadOnlyList<double>?>(nameof(BorderDashArray));

    /// <summary>
    /// Identifies the <see cref="BorderDashOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<double> BorderDashOffsetProperty =
        AvaloniaProperty.Register<AbstractPagination, double>(nameof(BorderDashOffset));

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractPagination>();
    
    /// <summary>
    /// Identifies the <see cref="CurrentPage" /> property.
    /// </summary>
    public static readonly StyledProperty<int> CurrentPageProperty =
        AvaloniaProperty.Register<AbstractPagination, int>(
            nameof(CurrentPage),
            DefaultCurrentPage,
            defaultBindingMode: BindingMode.TwoWay,
            validate:v => v > 0);
    
    /// <summary>
    /// Identifies the <see cref="PageSize" /> property.
    /// </summary>
    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<AbstractPagination, int>(
            nameof(PageSize),
            DefaultPageSize,
            defaultBindingMode: BindingMode.TwoWay,
            validate:PageSizeValidator);
    
    /// <summary>
    /// Identifies the <see cref="Total" /> property.
    /// </summary>
    public static readonly StyledProperty<int> TotalProperty =
        AvaloniaProperty.Register<AbstractPagination, int>(nameof(Total), validate:v => v >= 0);
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractPagination>();
    
    /// <summary>
    /// Identifies the <see cref="PageCount" /> property.
    /// </summary>
    public static readonly DirectProperty<AbstractPagination, int> PageCountProperty =
        AvaloniaProperty.RegisterDirect<AbstractPagination, int>(nameof(PageCount),
            o => o.PageCount,
            (o, v) => o.PageCount = v);
    
    /// <summary>
    /// Set to <see langword="true" /> to hide the pager content while <see cref="PageCount" /> is not greater
    /// than 1. With <see langword="false" />, the content is shown whenever <see cref="Total" /> is greater
    /// than 0; it stays hidden while <see cref="Total" /> is 0, whatever this property is.
    /// </summary>
    public bool IsHideOnSinglePage
    {
        get => GetValue(IsHideOnSinglePageProperty);
        set => SetValue(IsHideOnSinglePageProperty, value);
    }
    
    /// <summary>
    /// Horizontal placement of the pager within the width its parent provides; the theme maps the value to
    /// the <c>HorizontalAlignment</c> of the control. A cleared value falls back to
    /// <see cref="PaginationAlign.Start" />.
    /// </summary>
    public PaginationAlign Align
    {
        get => GetValue(AlignProperty);
        set => SetValue(AlignProperty, value);
    }
    
    /// <summary>
    /// Dash pattern of the stroke around the pager, as alternating dash and gap lengths. The theme applies
    /// it to the root border only; the page items are not affected.
    /// </summary>
    public IReadOnlyList<double>? BorderDashArray
    {
        get => GetValue(BorderDashArrayProperty);
        set => SetValue(BorderDashArrayProperty, value);
    }

    /// <summary>
    /// Offset into the dash pattern of <see cref="BorderDashArray" /> at which the stroke around the pager
    /// starts.
    /// </summary>
    public double BorderDashOffset
    {
        get => GetValue(BorderDashOffsetProperty);
        set => SetValue(BorderDashOffsetProperty, value);
    }

    /// <summary>
    /// Size preset of the pager. <c>Small</c> switches the page items, the gaps between the pager's elements
    /// and the quick jumper input to their small values. The value is passed down to the page items and the
    /// quick jumper, so they follow the size of the pager.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    /// <summary>
    /// Number of the page that is shown, counted from 1. A value beyond <see cref="PageCount" /> is replaced
    /// by the last page, which is 1 while there are no pages; 0 and negative values are rejected.
    /// </summary>
    public int CurrentPage
    {
        get => GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }
    
    /// <summary>
    /// Number of items on a page, which together with <see cref="Total" /> determines <see cref="PageCount" />.
    /// A value of 0 makes the page layout use <see cref="DefaultPageSize" />; negative values are rejected.
    /// </summary>
    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }
    
    /// <summary>
    /// Number of items the pager splits into pages. The pager content stays hidden while this is 0;
    /// negative values are rejected.
    /// </summary>
    public int Total
    {
        get => GetValue(TotalProperty);
        set => SetValue(TotalProperty, value);
    }
    
    /// <summary>
    /// Set to <see langword="true" /> to animate the state changes of the page items. The theme passes the
    /// value down to the items and takes its default from the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private int _pageCount;
    
    /// <summary>
    /// Number of pages that <see cref="Total" /> is split into on the basis of <see cref="PageSize" />;
    /// 0 while there are no items. It is updated before <see cref="CurrentPageChanged" /> is raised, and
    /// callers can only read it.
    /// </summary>
    public int PageCount
    {
        get => _pageCount;
        internal set => SetAndRaise(PageCountProperty, ref _pageCount, value);
    }
    
    #endregion
    
    #region 公共事件定义

    /// <summary>
    /// Raised after the page layout has been recalculated and committed: whenever <see cref="Total" />,
    /// <see cref="PageSize" /> or <see cref="CurrentPage" /> changes, and when the pager's template is applied.
    /// <see cref="PageCount" /> then holds the new value and <see cref="CurrentPage" /> is already clamped.
    /// </summary>
    public event EventHandler<PageChangedEventArgs>? CurrentPageChanged;

    #endregion
    
    #region 内部属性定义
    
    internal static readonly DirectProperty<AbstractPagination, bool> IsEffectiveVisibleProperty =
        AvaloniaProperty.RegisterDirect<AbstractPagination, bool>(
            nameof(IsEffectiveVisible),
            o => o.IsEffectiveVisible,
            (o, v) => o.IsEffectiveVisible = v);
    
    private bool _isEffectiveVisible = false;
    internal bool IsEffectiveVisible
    {
        get => _isEffectiveVisible;
        set => SetAndRaise(IsEffectiveVisibleProperty, ref _isEffectiveVisible, value);
    }

    // Presentation inputs set from the terminal's own Tokens by its ControlTheme. They are read by the root
    // template and passed to the nav items and the quick jumper explicitly, so they do not inherit through
    // the whole pager tree.
    internal static readonly StyledProperty<double> ItemSizeProperty =
        AvaloniaProperty.Register<AbstractPagination, double>(nameof(ItemSize));

    internal static readonly StyledProperty<IBrush?> ItemActiveColorDisabledProperty =
        AvaloniaProperty.Register<AbstractPagination, IBrush?>(nameof(ItemActiveColorDisabled));

    internal static readonly StyledProperty<double> LayoutSpacingProperty =
        AvaloniaProperty.Register<AbstractPagination, double>(nameof(LayoutSpacing));

    internal static readonly StyledProperty<double> QuickJumperSpacingProperty =
        AvaloniaProperty.Register<AbstractPagination, double>(nameof(QuickJumperSpacing));

    internal static readonly StyledProperty<double> QuickJumperInputWidthProperty =
        AvaloniaProperty.Register<AbstractPagination, double>(nameof(QuickJumperInputWidth));

    internal static readonly StyledProperty<Thickness> ItemPaddingInlineProperty =
        AvaloniaProperty.Register<AbstractPagination, Thickness>(nameof(ItemPaddingInline));

    internal static readonly StyledProperty<IBrush?> ItemColorProperty =
        AvaloniaProperty.Register<AbstractPagination, IBrush?>(nameof(ItemColor));

    internal static readonly StyledProperty<IBrush?> ItemHoverColorProperty =
        AvaloniaProperty.Register<AbstractPagination, IBrush?>(nameof(ItemHoverColor));

    internal static readonly StyledProperty<IBrush?> ItemHoverBgProperty =
        AvaloniaProperty.Register<AbstractPagination, IBrush?>(nameof(ItemHoverBg));

    internal static readonly StyledProperty<CornerRadius> ItemBorderRadiusProperty =
        AvaloniaProperty.Register<AbstractPagination, CornerRadius>(nameof(ItemBorderRadius));

    internal double ItemSize
    {
        get => GetValue(ItemSizeProperty);
        set => SetValue(ItemSizeProperty, value);
    }

    internal IBrush? ItemActiveColorDisabled
    {
        get => GetValue(ItemActiveColorDisabledProperty);
        set => SetValue(ItemActiveColorDisabledProperty, value);
    }

    internal double LayoutSpacing
    {
        get => GetValue(LayoutSpacingProperty);
        set => SetValue(LayoutSpacingProperty, value);
    }

    internal double QuickJumperSpacing
    {
        get => GetValue(QuickJumperSpacingProperty);
        set => SetValue(QuickJumperSpacingProperty, value);
    }

    internal double QuickJumperInputWidth
    {
        get => GetValue(QuickJumperInputWidthProperty);
        set => SetValue(QuickJumperInputWidthProperty, value);
    }

    internal Thickness ItemPaddingInline
    {
        get => GetValue(ItemPaddingInlineProperty);
        set => SetValue(ItemPaddingInlineProperty, value);
    }

    internal IBrush? ItemColor
    {
        get => GetValue(ItemColorProperty);
        set => SetValue(ItemColorProperty, value);
    }

    internal IBrush? ItemHoverColor
    {
        get => GetValue(ItemHoverColorProperty);
        set => SetValue(ItemHoverColorProperty, value);
    }

    internal IBrush? ItemHoverBg
    {
        get => GetValue(ItemHoverBgProperty);
        set => SetValue(ItemHoverBgProperty, value);
    }

    internal CornerRadius ItemBorderRadius
    {
        get => GetValue(ItemBorderRadiusProperty);
        set => SetValue(ItemBorderRadiusProperty, value);
    }

    #endregion
    
    /// <summary>
    /// Whether the template of the derived pager is applied, so that it may update its items. This class
    /// only stores the value; the derived pagers set and read it.
    /// </summary>
    protected bool TemplateConfigured = false;
    
    static AbstractPagination()
    {
        AffectsMeasure<AbstractPagination>(SizeTypeProperty);
    }

    /// <summary>
    /// Called by Avalonia when a property of the pager changes. The base implementation recalculates the page
    /// layout after <see cref="Total" />, <see cref="PageSize" /> or <see cref="CurrentPage" /> changed, and
    /// the effective visibility after <see cref="IsHideOnSinglePage" />, <see cref="PageCount" /> or
    /// <see cref="Total" /> changed. Overrides must call the base member to keep both current.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TotalProperty ||
            change.Property == PageSizeProperty ||
            change.Property == CurrentPageProperty)
        {
            HandlePageConditionChanged();
        }
        if (change.Property == IsHideOnSinglePageProperty ||
            change.Property == PageCountProperty ||
            change.Property == TotalProperty)
        {
            if (Total > 0)
            {
                if (IsHideOnSinglePage)
                {
                    SetValue(IsEffectiveVisibleProperty, PageCount > 1);
                }
                else
                {
                    SetValue(IsEffectiveVisibleProperty, true);
                }
            }
            else
            {
                SetValue(IsEffectiveVisibleProperty, false);
            }
        }
    }

    /// <summary>
    /// Recalculates <see cref="PageCount" /> from <see cref="Total" /> and <see cref="PageSize" />, and
    /// clamps <see cref="CurrentPage" /> into the range of existing pages. When the page had to be clamped,
    /// the method returns early and the resulting <see cref="CurrentPage" /> change runs it a second time,
    /// which then stores <see cref="PageCount" /> and calls <see cref="NotifyPageConditionChanged" />.
    /// A derived pager calls it once its template is applied so that its items show the initial state.
    /// </summary>
    protected void HandlePageConditionChanged()
    {
        var total       = Math.Max(0, Total);
        var pageSize    = PageSize <= 0 ? DefaultPageSize : PageSize;
        var pageCount   = (int)Math.Ceiling(total / (double)pageSize);
        var currentPage = Math.Max(1, Math.Min(CurrentPage, pageCount));
        if (CurrentPage != currentPage)
        {
            SetCurrentValue(CurrentPageProperty, currentPage);
            return;
        }

        PageCount = pageCount;
        NotifyPageConditionChanged(currentPage, pageCount, pageSize, total);
    }

    /// <summary>
    /// Called by <see cref="HandlePageConditionChanged" /> once <see cref="CurrentPage" /> is clamped and
    /// <see cref="PageCount" /> is stored. The base implementation raises <see cref="CurrentPageChanged" />;
    /// an override must call it to keep the event.
    /// </summary>
    /// <param name="currentPage">
    /// The clamped current page; 1 when there are no pages.
    /// </param>
    /// <param name="pageCount">
    /// The new number of pages; 0 while <see cref="Total" /> is 0.
    /// </param>
    /// <param name="pageSize">
    /// The page size used for the calculation; <see cref="DefaultPageSize" /> while
    /// <see cref="PageSize" /> is 0.
    /// </param>
    /// <param name="total">
    /// The item count used for the calculation; never negative.
    /// </param>
    protected virtual void NotifyPageConditionChanged(int currentPage, int pageCount, int pageSize, long total)
    {
        EmitCurrentPageChanged(CurrentPage, pageCount, pageSize);
    }

    /// <summary>
    /// Raises <see cref="CurrentPageChanged" /> with the given page layout.
    /// </summary>
    /// <param name="currentPage">
    /// The current page, counted from 1.
    /// </param>
    /// <param name="pageCount">
    /// The number of pages.
    /// </param>
    /// <param name="pageSize">
    /// The number of items on a page.
    /// </param>
    protected void EmitCurrentPageChanged(int currentPage, int pageCount, int pageSize)
    {
        CurrentPageChanged?.Invoke(this, new PageChangedEventArgs(currentPage, pageCount, pageSize));
    }

    private static bool PageSizeValidator(int pageSize)
    {
        return pageSize >= 0;
    }
}
