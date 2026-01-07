using System.Reactive.Disposables;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input.Raw;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public class AbstractSelect : TemplatedControl,
                              IMotionAwareControl,
                              ISizeTypeAware
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsAllowClearProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsAllowClear));
    
    public static readonly StyledProperty<bool> IsAutoClearSearchValueProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsAutoClearSearchValue));
    
    public static readonly StyledProperty<bool> IsDefaultOpenProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsDefaultOpen));
    
    public static readonly StyledProperty<bool> IsDropDownOpenProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsDropDownOpen));
    
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<AbstractSelect, string?>(nameof(PlaceholderText));
    
    public static readonly StyledProperty<IBrush?> PlaceholderForegroundProperty =
        AvaloniaProperty.Register<AbstractSelect, IBrush?>(nameof(PlaceholderForeground));
    
    public static readonly StyledProperty<bool> IsPopupMatchSelectWidthProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsPopupMatchSelectWidth), true);
    
    public static readonly StyledProperty<bool> IsFilterEnabledProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsFilterEnabled));
    
    public static readonly StyledProperty<int> DisplayPageSizeProperty = 
        AvaloniaProperty.Register<AbstractSelect, int>(nameof (DisplayPageSize), 10);
    
    public static readonly StyledProperty<int> MaxCountProperty =
        AvaloniaProperty.Register<AbstractSelect, int>(nameof(MaxCount), int.MaxValue);
    
    public static readonly StyledProperty<bool> IsShowMaxCountIndicatorProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsShowMaxCountIndicator));
    
    public static readonly StyledProperty<int?> MaxTagCountProperty =
        AvaloniaProperty.Register<AbstractSelect, int?>(nameof(MaxTagCount));
    
    public static readonly StyledProperty<bool?> IsResponsiveMaxTagCountProperty =
        AvaloniaProperty.Register<AbstractSelect, bool?>(nameof(IsResponsiveMaxTagCount));
    
    public static readonly StyledProperty<string?> MaxTagPlaceholderProperty =
        AvaloniaProperty.Register<AbstractSelect, string?>(nameof(MaxTagPlaceholder));
    
    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AddOnDecoratedBox.LeftAddOnProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AddOnDecoratedBox.LeftAddOnTemplateProperty.AddOwner<AbstractSelect>();

    public static readonly StyledProperty<object?> RightAddOnProperty =
        AddOnDecoratedBox.RightAddOnProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AddOnDecoratedBox.RightAddOnTemplateProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<object?> ContentLeftAddOnProperty =
        AddOnDecoratedBox.ContentLeftAddOnProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentLeftAddOnTemplateProperty =
        AddOnDecoratedBox.ContentLeftAddOnTemplateProperty.AddOwner<AbstractSelect>();

    public static readonly StyledProperty<object?> ContentRightAddOnProperty =
        AddOnDecoratedBox.ContentRightAddOnProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<IDataTemplate?> ContentRightAddOnTemplateProperty =
        AddOnDecoratedBox.ContentRightAddOnTemplateProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<AddOnDecoratedVariant> StyleVariantProperty =
        AddOnDecoratedBox.StyleVariantProperty.AddOwner<AbstractSelect>();

    public static readonly StyledProperty<AddOnDecoratedStatus> StatusProperty =
        AddOnDecoratedBox.StatusProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<SelectPopupPlacement> PlacementProperty =
        AvaloniaProperty.Register<AbstractSelect, SelectPopupPlacement>(nameof(Placement));
    
    public static readonly StyledProperty<object?> FilterValueProperty =
        AvaloniaProperty.Register<AbstractSelect, object?>(nameof(FilterValue));
    
    public static readonly StyledProperty<PathIcon?> SuffixIconProperty =
        AvaloniaProperty.Register<AbstractSelect, PathIcon?>(nameof(SuffixIcon));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractSelect>();
    
    public static readonly StyledProperty<bool> IsOperatingProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsOperating));
    
    public static readonly StyledProperty<string?> OperatingMsgProperty =
        AvaloniaProperty.Register<AbstractSelect, string?>(nameof(OperatingMsg));
    
    public static readonly StyledProperty<object?> CustomOperatingIndicatorProperty =
        AvaloniaProperty.Register<AbstractSelect, object?>(nameof(CustomOperatingIndicator));

    public static readonly StyledProperty<IDataTemplate?> CustomOperatingIndicatorTemplateProperty =
        AvaloniaProperty.Register<AbstractSelect, IDataTemplate?>(nameof(CustomOperatingIndicatorTemplate));
    
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<AbstractSelect, object?>(nameof(EmptyIndicator));
    
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<AbstractSelect, IDataTemplate?>(nameof(EmptyIndicatorTemplate));
    
    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsShowEmptyIndicator), true);
    
    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<AbstractSelect, Thickness>(nameof(EmptyIndicatorPadding));
    
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsLoading));
    
    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }
    
    public bool IsAutoClearSearchValue
    {
        get => GetValue(IsAutoClearSearchValueProperty);
        set => SetValue(IsAutoClearSearchValueProperty, value);
    }
    
    public bool IsDefaultOpen
    {
        get => GetValue(IsDefaultOpenProperty);
        set => SetValue(IsDefaultOpenProperty, value);
    }
    
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }
    
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }
    
    public IBrush? PlaceholderForeground
    {
        get => GetValue(PlaceholderForegroundProperty);
        set => SetValue(PlaceholderForegroundProperty, value);
    }
    
    public bool IsPopupMatchSelectWidth
    {
        get => GetValue(IsPopupMatchSelectWidthProperty);
        set => SetValue(IsPopupMatchSelectWidthProperty, value);
    }
    
    public bool IsFilterEnabled
    {
        get => GetValue(IsFilterEnabledProperty);
        set => SetValue(IsFilterEnabledProperty, value);
    }
    
    public int DisplayPageSize
    {
        get => GetValue(DisplayPageSizeProperty);
        set => SetValue(DisplayPageSizeProperty, value);
    }
    
    public int MaxCount
    {
        get => GetValue(MaxCountProperty);
        set => SetValue(MaxCountProperty, value);
    }
    
    public bool IsShowMaxCountIndicator
    {
        get => GetValue(IsShowMaxCountIndicatorProperty);
        set => SetValue(IsShowMaxCountIndicatorProperty, value);
    }
    
    public int? MaxTagCount
    {
        get => GetValue(MaxTagCountProperty);
        set => SetValue(MaxTagCountProperty, value);
    }
    
    public bool? IsResponsiveMaxTagCount
    {
        get => GetValue(IsResponsiveMaxTagCountProperty);
        set => SetValue(IsResponsiveMaxTagCountProperty, value);
    }
    
    public string? MaxTagPlaceholder
    {
        get => GetValue(MaxTagPlaceholderProperty);
        set => SetValue(MaxTagPlaceholderProperty, value);
    }
    
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }
    
    public IDataTemplate? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }

    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }
    
    public IDataTemplate? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }
    
    public object? ContentLeftAddOn
    {
        get => GetValue(ContentLeftAddOnProperty);
        set => SetValue(ContentLeftAddOnProperty, value);
    }
    
    public IDataTemplate? ContentLeftAddOnTemplate
    {
        get => GetValue(ContentLeftAddOnTemplateProperty);
        set => SetValue(ContentLeftAddOnTemplateProperty, value);
    }

    public object? ContentRightAddOn
    {
        get => GetValue(ContentRightAddOnProperty);
        set => SetValue(ContentRightAddOnProperty, value);
    }
    
    public IDataTemplate? ContentRightAddOnTemplate
    {
        get => GetValue(ContentRightAddOnTemplateProperty);
        set => SetValue(ContentRightAddOnTemplateProperty, value);
    }
    
    public AddOnDecoratedVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    public AddOnDecoratedStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    
    public SelectPopupPlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    
    public object? FilterValue
    {
        get => GetValue(FilterValueProperty);
        set => SetValue(FilterValueProperty, value);
    }
    
    public PathIcon? SuffixIcon
    {
        get => GetValue(SuffixIconProperty);
        set => SetValue(SuffixIconProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public bool IsOperating
    {
        get => GetValue(IsOperatingProperty);
        set => SetValue(IsOperatingProperty, value);
    }
    
    public string? OperatingMsg
    {
        get => GetValue(OperatingMsgProperty);
        set => SetValue(OperatingMsgProperty, value);
    }
    
    [DependsOn(nameof(CustomOperatingIndicatorTemplate))]
    public object? CustomOperatingIndicator
    {
        get => GetValue(CustomOperatingIndicatorProperty);
        set => SetValue(CustomOperatingIndicatorProperty, value);
    }
    
    public IDataTemplate? CustomOperatingIndicatorTemplate
    {
        get => GetValue(CustomOperatingIndicatorTemplateProperty);
        set => SetValue(CustomOperatingIndicatorTemplateProperty, value);
    }
    
    [DependsOn(nameof(EmptyIndicatorTemplate))]
    public object? EmptyIndicator
    {
        get => GetValue(EmptyIndicatorProperty);
        set => SetValue(EmptyIndicatorProperty, value);
    }

    public IDataTemplate? EmptyIndicatorTemplate
    {
        get => GetValue(EmptyIndicatorTemplateProperty);
        set => SetValue(EmptyIndicatorTemplateProperty, value);
    }
    
    public bool IsShowEmptyIndicator
    {
        get => GetValue(IsShowEmptyIndicatorProperty);
        set => SetValue(IsShowEmptyIndicatorProperty, value);
    }
    
    public Thickness EmptyIndicatorPadding
    {
        get => GetValue(EmptyIndicatorPaddingProperty);
        set => SetValue(EmptyIndicatorPaddingProperty, value);
    }
    
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
    #endregion
    
    #region 公共事件定义
    
    public event EventHandler? DropDownClosed;
    public event EventHandler? DropDownOpened;

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<AbstractSelect, double>(nameof(ItemHeight));
    
    internal static readonly StyledProperty<double> MaxPopupHeightProperty =
        AvaloniaProperty.Register<AbstractSelect, double>(nameof(MaxPopupHeight));
    
    internal static readonly StyledProperty<Thickness> PopupContentPaddingProperty =
        AvaloniaProperty.Register<AbstractSelect, Thickness>(nameof(PopupContentPadding));
    
    internal static readonly DirectProperty<AbstractSelect, bool> IsEffectiveShowClearButtonProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, bool>(nameof(IsEffectiveShowClearButton),
            o => o.IsEffectiveShowClearButton,
            (o, v) => o.IsEffectiveShowClearButton = v);
    
    internal static readonly DirectProperty<AbstractSelect, double> EffectivePopupWidthProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, double>(
            nameof(EffectivePopupWidth),
            o => o.EffectivePopupWidth,
            (o, v) => o.EffectivePopupWidth = v);
    
    internal static readonly DirectProperty<AbstractSelect, bool> IsPlaceholderTextVisibleProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, bool>(
            nameof(IsPlaceholderTextVisible),
            o => o.IsPlaceholderTextVisible,
            (o, v) => o.IsPlaceholderTextVisible = v);
    
    internal static readonly DirectProperty<AbstractSelect, bool> IsSelectionEmptyProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, bool>(
            nameof(IsSelectionEmpty),
            o => o.IsSelectionEmpty,
            (o, v) => o.IsSelectionEmpty = v);

    internal static readonly DirectProperty<AbstractSelect, int> SelectedCountProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, int>(nameof(SelectedCount),
            o => o.SelectedCount,
            (o, v) => o.SelectedCount = v);
    
    internal static readonly DirectProperty<AbstractSelect, string?> ActivateFilterValueProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, string?>(nameof(ActivateFilterValue),
            o => o.ActivateFilterValue,
            (o, v) => o.ActivateFilterValue = v);
    
    internal static readonly DirectProperty<AbstractSelect, bool> IsEffectiveFilterEnabledProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, bool>(nameof(IsEffectiveFilterEnabled),
            o => o.IsEffectiveFilterEnabled,
            (o, v) => o.IsEffectiveFilterEnabled = v);
    
    internal double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }
    
    internal double MaxPopupHeight
    {
        get => GetValue(MaxPopupHeightProperty);
        set => SetValue(MaxPopupHeightProperty, value);
    }
    
    internal Thickness PopupContentPadding
    {
        get => GetValue(PopupContentPaddingProperty);
        set => SetValue(PopupContentPaddingProperty, value);
    }
    
    private bool _isEffectiveShowClearButton;

    internal bool IsEffectiveShowClearButton
    {
        get => _isEffectiveShowClearButton;
        set => SetAndRaise(IsEffectiveShowClearButtonProperty, ref _isEffectiveShowClearButton, value);
    }
    
    private double _effectivePopupWidth;

    internal double EffectivePopupWidth
    {
        get => _effectivePopupWidth;
        set => SetAndRaise(EffectivePopupWidthProperty, ref _effectivePopupWidth, value);
    }
    
    private bool _isPlaceholderTextVisible;

    internal bool IsPlaceholderTextVisible
    {
        get => _isPlaceholderTextVisible;
        set => SetAndRaise(IsPlaceholderTextVisibleProperty, ref _isPlaceholderTextVisible, value);
    }
    
    private bool _isSelectionEmpty = true;

    internal bool IsSelectionEmpty
    {
        get => _isSelectionEmpty;
        set => SetAndRaise(IsSelectionEmptyProperty, ref _isSelectionEmpty, value);
    }

    private int _selectedCount;

    internal int SelectedCount
    {
        get => _selectedCount;
        set => SetAndRaise(SelectedCountProperty, ref _selectedCount, value);
    }
    
    private string? _activateFilterValue;

    internal string? ActivateFilterValue
    {
        get => _activateFilterValue;
        set => SetAndRaise(ActivateFilterValueProperty, ref _activateFilterValue, value);
    }
    
    private bool _isEffectiveFilterEnabled;

    internal bool IsEffectiveFilterEnabled
    {
        get => _isEffectiveFilterEnabled;
        set => SetAndRaise(IsEffectiveFilterEnabledProperty, ref _isEffectiveFilterEnabled, value);
    }
    #endregion
    
    private protected readonly CompositeDisposable SubscriptionsOnOpen = new ();
    private protected Popup? Popup;
    private protected bool IgnorePopupClose;

    protected void NotifyPopupClosed()
    {
        DropDownClosed?.Invoke(this, EventArgs.Empty);
    }
    
    protected void NotifyPopupOpened()
    {
        DropDownOpened?.Invoke(this, EventArgs.Empty);
    }
    
    protected bool PopupClosePredicate(IPopupHostProvider hostProvider, RawPointerEventArgs args)
    {
        if (IgnorePopupClose)
        {
            IgnorePopupClose = false;
            return false;
        }
        if (hostProvider.PopupHost is OverlayPopupHost overlayPopupHost && args.Root is Control root)
        {
            var offset = overlayPopupHost.TranslatePoint(default, root);
            if (offset.HasValue)
            {
                var bounds = new Rect(offset.Value, overlayPopupHost.Bounds.Size);
                return !bounds.Contains(args.Position);
            }
        }
                
        return false;
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window window)
        {
            window.Deactivated += HandleWindowDeactivated;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window window)
        {
            window.Deactivated -= HandleWindowDeactivated;
        }
    }

    private void HandleWindowDeactivated(object? sender, EventArgs e)
    {
        // SetCurrentValue(IsDropDownOpenProperty, false);
    }
}