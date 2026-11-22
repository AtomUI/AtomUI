using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public abstract class AbstractPagination : TemplatedControl, ICustomizableSizeTypeAware, IMotionAwareControl
{
    #region 公共属性定义

    public const int DefaultPageSize = 10;
    public const int DefaultCurrentPage = 1;

    public static readonly StyledProperty<bool> IsHideOnSinglePageProperty =
        AvaloniaProperty.Register<AbstractPagination, bool>(nameof(IsHideOnSinglePage));
    
    public static readonly StyledProperty<PaginationAlign> AlignProperty =
        AvaloniaProperty.Register<AbstractPagination, PaginationAlign>(nameof(Align));
    
    public static readonly StyledProperty<IReadOnlyList<double>?> BorderDashArrayProperty =
        AvaloniaProperty.Register<AbstractPagination, IReadOnlyList<double>?>(nameof(BorderDashArray));

    public static readonly StyledProperty<double> BorderDashOffsetProperty =
        AvaloniaProperty.Register<AbstractPagination, double>(nameof(BorderDashOffset));

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractPagination>();
    
    public static readonly StyledProperty<int> CurrentPageProperty =
        AvaloniaProperty.Register<AbstractPagination, int>(
            nameof(CurrentPage),
            DefaultCurrentPage,
            defaultBindingMode: BindingMode.TwoWay,
            validate:v => v > 0);
    
    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<AbstractPagination, int>(
            nameof(PageSize),
            DefaultPageSize,
            defaultBindingMode: BindingMode.TwoWay,
            validate:PageSizeValidator);
    
    public static readonly StyledProperty<int> TotalProperty =
        AvaloniaProperty.Register<AbstractPagination, int>(nameof(Total), validate:v => v >= 0);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractPagination>();
    
    public static readonly DirectProperty<AbstractPagination, int> PageCountProperty =
        AvaloniaProperty.RegisterDirect<AbstractPagination, int>(nameof(PageCount),
            o => o.PageCount,
            (o, v) => o.PageCount = v);
    
    public bool IsHideOnSinglePage
    {
        get => GetValue(IsHideOnSinglePageProperty);
        set => SetValue(IsHideOnSinglePageProperty, value);
    }
    
    public PaginationAlign Align
    {
        get => GetValue(AlignProperty);
        set => SetValue(AlignProperty, value);
    }
    
    public IReadOnlyList<double>? BorderDashArray
    {
        get => GetValue(BorderDashArrayProperty);
        set => SetValue(BorderDashArrayProperty, value);
    }

    public double BorderDashOffset
    {
        get => GetValue(BorderDashOffsetProperty);
        set => SetValue(BorderDashOffsetProperty, value);
    }

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public int CurrentPage
    {
        get => GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }
    
    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }
    
    public int Total
    {
        get => GetValue(TotalProperty);
        set => SetValue(TotalProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private int _pageCount;
    
    public int PageCount
    {
        get => _pageCount;
        internal set => SetAndRaise(PageCountProperty, ref _pageCount, value);
    }
    
    #endregion
    
    #region 公共事件定义

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

    // Presentation inputs set from the terminal's own Tokens by its ControlTheme. They are read by the root template and
    // passed to the nav items and the quick jumper explicitly, so they do not inherit through the whole pager tree.
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
    
    protected bool TemplateConfigured = false;
    
    static AbstractPagination()
    {
        AffectsMeasure<AbstractPagination>(SizeTypeProperty);
    }

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

    protected virtual void NotifyPageConditionChanged(int currentPage, int pageCount, int pageSize, long total)
    {
        EmitCurrentPageChanged(CurrentPage, pageCount, pageSize);
    }

    protected void EmitCurrentPageChanged(int currentPage, int pageCount, int pageSize)
    {
        CurrentPageChanged?.Invoke(this, new PageChangedEventArgs(currentPage, pageCount, pageSize));
    }

    private static bool PageSizeValidator(int pageSize)
    {
        return pageSize >= 0;
    }
}
