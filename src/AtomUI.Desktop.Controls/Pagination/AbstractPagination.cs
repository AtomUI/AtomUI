using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

public abstract class AbstractPagination : TemplatedControl, ISizeTypeAware, IMotionAwareControl
{
    public const int DefaultPageSize = 10;
    public const int DefaultCurrentPage = 1;
    
    #region 公共属性定义
    
    public static readonly StyledProperty<bool> IsHideOnSinglePageProperty =
        AvaloniaProperty.Register<AbstractPagination, bool>(nameof(IsHideOnSinglePage));
    
    public static readonly StyledProperty<PaginationAlign> AlignProperty =
        AvaloniaProperty.Register<AbstractPagination, PaginationAlign>(nameof(PaginationAlign));
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractPagination>();
    
    public static readonly StyledProperty<int> CurrentPageProperty =
        AvaloniaProperty.Register<AbstractPagination, int>(nameof(CurrentPage), DefaultCurrentPage,
            validate:v => v > 0);
    
    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<AbstractPagination, int>(nameof(PageSize), DefaultPageSize,
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
    
    public SizeType SizeType
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

    private static bool PageSizeValidator(int pageSize)
    {
        int[] allowPageSizes = [0, 10, 20, 50, 100];
        return allowPageSizes.Contains(pageSize);
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
        CurrentPage = currentPage;
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
}