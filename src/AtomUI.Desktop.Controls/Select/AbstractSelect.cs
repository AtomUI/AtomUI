using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

public abstract class AbstractSelect : TemplatedControl,
                                       IMotionAwareControl,
                                       ICustomizableSizeTypeAware,
                                       ICompactSpaceAware,
                                       IInputControlStatusAware,
                                       IInputControlStyleVariantAware,
                                       IFormItemAware,
                                       IFormItemFeedbackAware
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

    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(ShouldUseOverlayPopup), true);

    public static readonly StyledProperty<bool> IsFilterEnabledProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsFilterEnabled));

    public static readonly StyledProperty<bool> IsShowOverflowTipProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsShowOverflowTip), true);

    public static readonly StyledProperty<int> OverflowTipDelayProperty =
        AvaloniaProperty.Register<AbstractSelect, int>(nameof(OverflowTipDelay), 1200);

    public static readonly StyledProperty<PlacementMode> OverflowTipPlacementProperty =
        AvaloniaProperty.Register<AbstractSelect, PlacementMode>(nameof(OverflowTipPlacement), PlacementMode.TopEdgeAlignedLeft);

    public static readonly StyledProperty<int> DisplayPageSizeProperty =
        AvaloniaProperty.Register<AbstractSelect, int>(nameof (DisplayPageSize), 10);

    public static readonly StyledProperty<int> MaxCountProperty =
        AvaloniaProperty.Register<AbstractSelect, int>(nameof(MaxCount), int.MaxValue);

    public static readonly StyledProperty<bool> IsShowMaxCountIndicatorProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsShowMaxCountIndicator));

    public static readonly StyledProperty<int?> MaxTagCountProperty =
        AvaloniaProperty.Register<AbstractSelect, int?>(nameof(MaxTagCount));

    public static readonly StyledProperty<bool> IsResponsiveTagModeProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsResponsiveTagMode));

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

    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<AbstractSelect>();

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<AbstractSelect>();

    public static readonly StyledProperty<SelectPopupPlacement> PlacementProperty =
        AvaloniaProperty.Register<AbstractSelect, SelectPopupPlacement>(nameof(Placement), SelectPopupPlacement.BottomEdgeAlignedLeft);

    public static readonly StyledProperty<object?> FilterValueProperty =
        AvaloniaProperty.Register<AbstractSelect, object?>(nameof(FilterValue));

    public static readonly StyledProperty<PathIcon?> ClearIconProperty =
        AvaloniaProperty.Register<AbstractSelect, PathIcon?>(nameof(ClearIcon));

    public static readonly StyledProperty<PathIcon?> SuffixIconProperty =
        AvaloniaProperty.Register<AbstractSelect, PathIcon?>(nameof(SuffixIcon));

    public static readonly StyledProperty<PathIcon?> SuffixLoadingIconProperty =
        AvaloniaProperty.Register<AbstractSelect, PathIcon?>(nameof(SuffixLoadingIcon));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractSelect>();

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractSelect>();

    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<AbstractSelect, object?>(nameof(EmptyIndicator));

    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<AbstractSelect, IDataTemplate?>(nameof(EmptyIndicatorTemplate));

    public static readonly StyledProperty<bool> IsShowEmptyIndicatorProperty =
        AvaloniaProperty.Register<AbstractSelect, bool>(nameof(IsShowEmptyIndicator), true);

    public static readonly StyledProperty<Thickness> EmptyIndicatorPaddingProperty =
        AvaloniaProperty.Register<AbstractSelect, Thickness>(nameof(EmptyIndicatorPadding));

    public static readonly DirectProperty<AbstractSelect, bool> IsLoadingProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, bool>(
            nameof(IsLoading),
            o => o.IsLoading);

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

    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    public bool IsFilterEnabled
    {
        get => GetValue(IsFilterEnabledProperty);
        set => SetValue(IsFilterEnabledProperty, value);
    }

    public bool IsShowOverflowTip
    {
        get => GetValue(IsShowOverflowTipProperty);
        set => SetValue(IsShowOverflowTipProperty, value);
    }

    public int OverflowTipDelay
    {
        get => GetValue(OverflowTipDelayProperty);
        set => SetValue(OverflowTipDelayProperty, value);
    }

    public PlacementMode OverflowTipPlacement
    {
        get => GetValue(OverflowTipPlacementProperty);
        set => SetValue(OverflowTipPlacementProperty, value);
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

    /// <summary>
    /// 响应式最大显示的 Tags 数量
    /// </summary>
    public int? MaxTagCount
    {
        get => GetValue(MaxTagCountProperty);
        set => SetValue(MaxTagCountProperty, value);
    }

    public bool IsResponsiveTagMode
    {
        get => GetValue(IsResponsiveTagModeProperty);
        set => SetValue(IsResponsiveTagModeProperty, value);
    }

    /// <summary>
    /// 在响应式情况下，Tags 因为宽度不够被隐藏之后显示的内容
    /// </summary>
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

    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    public InputControlStatus Status
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

    public PathIcon? ClearIcon
    {
        get => GetValue(ClearIconProperty);
        set => SetValue(ClearIconProperty, value);
    }

    public PathIcon? SuffixIcon
    {
        get => GetValue(SuffixIconProperty);
        set => SetValue(SuffixIconProperty, value);
    }

    public PathIcon? SuffixLoadingIcon
    {
        get => GetValue(SuffixLoadingIconProperty);
        set => SetValue(SuffixLoadingIconProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
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

    private bool _isLoading;

    public bool IsLoading
    {
        get => _isLoading;
        protected set => SetAndRaise(IsLoadingProperty, ref _isLoading, value);
    }
    #endregion

    #region 公共事件定义

    public event EventHandler<CancelEventArgs>? DropDownOpening;
    public event EventHandler? DropDownOpened;
    public event EventHandler<CancelEventArgs>? DropDownClosing;
    public event EventHandler? DropDownClosed;

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<AbstractSelect, double> ItemHeightProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, double>(
            nameof(ItemHeight),
            o => o.ItemHeight,
            (o, v) => o.ItemHeight = v);

    internal static readonly DirectProperty<AbstractSelect, double> MaxPopupHeightProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, double>(
            nameof(MaxPopupHeight),
            o => o.MaxPopupHeight,
            (o, v) => o.MaxPopupHeight = v);

    internal static readonly DirectProperty<AbstractSelect, Thickness> PopupContentPaddingProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, Thickness>(nameof(PopupContentPadding),
            o => o.PopupContentPadding,
            (o, v) => o.PopupContentPadding = v);

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

    internal static readonly DirectProperty<AbstractSelect, bool> IsSingleResultVisibleProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, bool>(
            nameof(IsSingleResultVisible),
            o => o.IsSingleResultVisible,
            (o, v) => o.IsSingleResultVisible = v);

    internal static readonly DirectProperty<AbstractSelect, bool> IsSelectionEmptyProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, bool>(
            nameof(IsSelectionEmpty),
            o => o.IsSelectionEmpty,
            (o, v) => o.IsSelectionEmpty = v);

    internal static readonly DirectProperty<AbstractSelect, int> SelectedCountProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, int>(nameof(SelectedCount),
            o => o.SelectedCount,
            (o, v) => o.SelectedCount = v);

    internal static readonly DirectProperty<AbstractSelect, PlacementMode> PopupPlacementProperty =
        AvaloniaProperty.RegisterDirect<AbstractSelect, PlacementMode>(
            nameof(PopupPlacement),
            o => o.PopupPlacement,
            (o, v) => o.PopupPlacement = v);

    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty =
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<AbstractSelect>();

    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty =
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<AbstractSelect>();

    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty =
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<AbstractSelect>();

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<AbstractSelect>();

    internal static readonly StyledProperty<FormValidateFeedback?> FormFeedbackProperty =
        AvaloniaProperty.Register<AbstractSelect, FormValidateFeedback?>(nameof(FormFeedback));

    /// <summary>
    /// 钉住候选弹层：开启后弹层忽略 light-dismiss 关闭请求，保持强制打开。
    /// 典型场景是 Gallery 语义部件预览需要持续高亮 popup.* 部件。
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<AbstractSelect>();

    private double _itemHeight;

    internal double ItemHeight
    {
        get => _itemHeight;
        set => SetAndRaise(ItemHeightProperty, ref _itemHeight, value);
    }

    private double _maxPopupHeight;

    internal double MaxPopupHeight
    {
        get => _maxPopupHeight;
        set => SetAndRaise(MaxPopupHeightProperty, ref _maxPopupHeight, value);
    }

    private Thickness _popupContentPadding;

    internal Thickness PopupContentPadding
    {
        get => _popupContentPadding;
        set => SetAndRaise(PopupContentPaddingProperty, ref _popupContentPadding, value);
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

    private bool _isSingleResultVisible;

    internal bool IsSingleResultVisible
    {
        get => _isSingleResultVisible;
        set => SetAndRaise(IsSingleResultVisibleProperty, ref _isSingleResultVisible, value);
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

    private PlacementMode _popupPlacement = PlacementMode.BottomEdgeAlignedLeft;

    internal PlacementMode PopupPlacement
    {
        get => _popupPlacement;
        set => SetAndRaise(PopupPlacementProperty, ref _popupPlacement, value);
    }

    internal SpaceItemPosition? CompactSpaceItemPosition
    {
        get => GetValue(CompactSpaceItemPositionProperty);
        set => SetValue(CompactSpaceItemPositionProperty, value);
    }

    internal Orientation CompactSpaceOrientation
    {
        get => GetValue(CompactSpaceOrientationProperty);
        set => SetValue(CompactSpaceOrientationProperty, value);
    }

    internal bool IsUsedInCompactSpace
    {
        get => GetValue(IsUsedInCompactSpaceProperty);
        set => SetValue(IsUsedInCompactSpaceProperty, value);
    }

    internal FormValidateStatus FormStatus
    {
        get => GetValue(FormStatusProperty);
        private set => SetCurrentValue(FormStatusProperty, value);
    }

    internal FormValidateFeedback? FormFeedback
    {
        get => GetValue(FormFeedbackProperty);
        set => SetValue(FormFeedbackProperty, value);
    }

    /// <summary>
    /// 钉住候选弹层：开启后弹层忽略 light-dismiss 关闭请求，保持强制打开。
    /// 典型场景是 Gallery 语义部件预览需要持续高亮 popup.* 部件。
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }
    #endregion

    private protected readonly CompositeDisposable SubscriptionsOnOpen = new ();
    private protected Popup? Popup;
    private protected bool PopupHasOpened;
    private protected bool IgnorePropertyChange;
    private AddOnDecoratedBox? _addOnDecoratedBox;
    private bool _isFrameBorderBrushRelayed;
    private bool _isFrameBackgroundRelayed;

    private IDisposable? _deactivationSubscription;
    private IDisposable? _popupPinnedOpenBinding;
    private EventHandler? _formValueChanged;

    static AbstractSelect()
    {
        AffectsArrange<AbstractSelect>(CompactSpaceItemPositionProperty, CompactSpaceOrientationProperty);
        IsDropDownOpenProperty.Changed.AddClassHandler<AbstractSelect>((select, args) => select.HandleIsDropDownOpenChanged(args));
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        ConfigurePopupPlacement();
        if (SuffixIcon == null)
        {
            SetCurrentValue(SuffixIconProperty, new DownOutlined());
        }

        if (SuffixLoadingIcon == null)
        {
            SetCurrentValue(SuffixLoadingIconProperty, new LoadingOutlined()
            {
                LoadingAnimation = IconAnimation.Spin
            });
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _deactivationSubscription =
            TopLevelDeactivation.Subscribe(TopLevel.GetTopLevel(this), HandleWindowDeactivated);
        if (IsPopupPinnedOpen && IsDropDownOpen)
        {
            OpenDropDown();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _deactivationSubscription?.Dispose();
        _deactivationSubscription = null;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureMaxDropdownHeight();
        if (Popup != null)
        {
            _popupPinnedOpenBinding?.Dispose();
            _popupPinnedOpenBinding = null;
            Popup.Opened -= PopupOpened;
            Popup.Closed -= PopupClosed;
            Popup.OverlayInputPassThroughElement = null;
        }

        Popup = e.NameScope.Find<Popup>("PART_Popup");
        _addOnDecoratedBox = e.NameScope.Find<AddOnDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);
        _isFrameBorderBrushRelayed = false;
        _isFrameBackgroundRelayed  = false;
        RelayRootSurfaceBrush(BorderBrushProperty);
        RelayRootSurfaceBrush(BackgroundProperty);

        if (Popup != null)
        {
            _popupPinnedOpenBinding = BindUtils.RelayBind(
                this,
                IsPopupPinnedOpenProperty,
                Popup,
                Popup.IsPopupPinnedOpenProperty);
            Popup.Opened += PopupOpened;
            Popup.Closed += PopupClosed;
        }

        // If the drop down property indicates that the popup is open,
        // flip its value to invoke the changed handler.
        if (IsDropDownOpen && Popup != null && !Popup.IsOpen)
        {
            OpeningDropDown(false);
        }
    }

    /// <summary>
    /// Relays the owner's root surface brushes onto the input frame as local values so
    /// application-level customization wins over the frame state machine, mirroring the input
    /// family (<c>AbstractTextInput.RelayRootSurfaceBrush</c>) and the upstream root
    /// border/background semantics. An unset owner value restores the state machine.
    /// </summary>
    private void RelayRootSurfaceBrush(AvaloniaProperty property)
    {
        var decoratedBox = _addOnDecoratedBox;
        if (decoratedBox is null)
        {
            return;
        }

        var value = GetValue(property);
        if (value is null || ReferenceEquals(value, AvaloniaProperty.UnsetValue))
        {
            // Only hand the property back when this control is the one that took it over: a
            // nested owner may hold its own relay on the same frame node.
            if (IsRelayed(property))
            {
                decoratedBox.ClearValue(property);
                SetRelayed(property, false);
            }

            return;
        }

        decoratedBox.SetValue(property, value, BindingPriority.LocalValue);
        SetRelayed(property, true);
    }

    private bool IsRelayed(AvaloniaProperty property)
    {
        return ReferenceEquals(property, BorderBrushProperty)
            ? _isFrameBorderBrushRelayed
            : _isFrameBackgroundRelayed;
    }

    private void SetRelayed(AvaloniaProperty property, bool value)
    {
        if (ReferenceEquals(property, BorderBrushProperty))
        {
            _isFrameBorderBrushRelayed = value;
        }
        else
        {
            _isFrameBackgroundRelayed = value;
        }
    }

    protected virtual void PopupClosed(object? sender, EventArgs e)
    {
        SubscriptionsOnOpen.Clear();
        PopupHasOpened = false;
        // 弹层关闭（含 light-dismiss 路径）时强制回写宿主状态，
        // 保持 IsDropDownOpen 与弹层实际状态同步（与 AbstractAutoComplete 一致）。
        if (IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
        NotifyPopupClosed();
    }

    protected virtual void PopupOpened(object? sender, EventArgs e)
    {
        SubscriptionsOnOpen.Clear();
        this.GetObservable(IsVisibleProperty).Subscribe(IsVisibleChanged).DisposeWith(SubscriptionsOnOpen);
        this.SubscribeAncestorIsVisible(IsVisibleChanged, SubscriptionsOnOpen);

        ConfigurePopupMinWith(Bounds.Width);
        NotifyPopupOpened();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BorderBrushProperty || change.Property == BackgroundProperty)
        {
            RelayRootSurfaceBrush(change.Property);
        }
        else if (change.Property == StatusProperty ||
            change.Property == FormStatusProperty ||
            change.Property == DataValidationErrors.HasErrorsProperty ||
            change.Property == DataValidationErrors.ErrorsProperty)
        {
            UpdatePseudoClasses();
        }
        else if (change.Property == PlacementProperty)
        {
            ConfigurePopupPlacement();
        }
        else if (change.Property == DisplayPageSizeProperty ||
                 change.Property == ItemHeightProperty)
        {
            ConfigureMaxDropdownHeight();
        }
        else if (change.Property == IsPopupPinnedOpenProperty)
        {
            if (change.GetNewValue<bool>() && !IsDropDownOpen)
            {
                SetCurrentValue(IsDropDownOpenProperty, true);
            }
        }
    }

    protected virtual void ConfigurePopupMinWith(double selectWidth)
    {
        if (IsPopupMatchSelectWidth)
        {
            SetCurrentValue(EffectivePopupWidthProperty, selectWidth);
        }
        else
        {
            SetCurrentValue(EffectivePopupWidthProperty, 0.0);
        }
    }

    protected virtual void ConfigureMaxDropdownHeight()
    {
        MaxPopupHeight = ItemHeight * DisplayPageSize + PopupContentPadding.Top + PopupContentPadding.Bottom;
    }

    protected void UpdatePseudoClasses()
    {
        var effectiveStatus = InputControlState.ResolveEffectiveStatus(this, Status, FormStatus);
        PseudoClasses.Set(SelectPseudoClass.DropdownOpen, IsDropDownOpen);
        PseudoClasses.Set(StdPseudoClass.Warning,
            effectiveStatus == InputControlStatus.Warning);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Outline, StyleVariant == InputControlStyleVariant.Outlined);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Filled, StyleVariant == InputControlStyleVariant.Filled);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Borderless, StyleVariant == InputControlStyleVariant.Borderless);
    }

    private protected void SetDropDownOpenWithoutPropertyHandling(bool isOpen)
    {
        IgnorePropertyChange = true;
        try
        {
            SetCurrentValue(IsDropDownOpenProperty, isOpen);
        }
        finally
        {
            if (IgnorePropertyChange)
            {
                IgnorePropertyChange = false;
            }
        }
    }

    protected void ClosingDropDown(bool oldValue)
    {
        var args = new CancelEventArgs();
        NotifyDropDownClosing(args);

        if (args.Cancel)
        {
            SetDropDownOpenWithoutPropertyHandling(oldValue);
        }
        else
        {
            CloseDropDown();
        }

        UpdatePseudoClasses();
    }

    protected void CloseDropDown()
    {
        if (PopupHasOpened)
        {
            if (Popup != null)
            {
                Popup.IsOpen = false;
            }
        }
    }

    protected void OpenDropDown()
    {
        EnsurePopupContent();
        if (Popup != null)
        {
            Popup.IsOpen = true;
        }
        PopupHasOpened = true;
    }

    protected virtual void EnsurePopupContent()
    {
    }

    protected void OpeningDropDown(bool oldValue)
    {
        var args = new CancelEventArgs();

        // Opening
        NotifyDropDownOpening(args);

        if (args.Cancel)
        {
            SetDropDownOpenWithoutPropertyHandling(oldValue);
        }
        else
        {
            OpenDropDown();
        }

        UpdatePseudoClasses();
    }

    protected virtual void NotifyPopupClosed()
    {
        DropDownClosed?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifyPopupOpened()
    {
        DropDownOpened?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifyDropDownOpening(CancelEventArgs eventArgs)
    {
        DropDownOpening?.Invoke(this, eventArgs);
    }

    protected virtual void NotifyDropDownOpened(EventArgs eventArgs)
    {
        DropDownOpened?.Invoke(this, eventArgs);
    }

    protected virtual void NotifyDropDownClosing(CancelEventArgs eventArgs)
    {
        DropDownClosing?.Invoke(this, eventArgs);
    }

    protected virtual void NotifyDropDownClosed(EventArgs eventArgs)
    {
        DropDownClosed?.Invoke(this, eventArgs);
    }

    #region 实现 CompactSpace 接口

    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        var isUsedInCompactSpace = position != null;
        if (IsUsedInCompactSpace != isUsedInCompactSpace)
        {
            IsUsedInCompactSpace = isUsedInCompactSpace;
        }

        if (CompactSpaceItemPosition != position)
        {
            CompactSpaceItemPosition = position;
        }
    }

    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        if (CompactSpaceOrientation != orientation)
        {
            CompactSpaceOrientation = orientation;
        }
    }

    double ICompactSpaceAware.GetBorderThickness()
    {
        return GetBorderThicknessForCompactSpace();
    }

    protected virtual double GetBorderThicknessForCompactSpace()
    {
        if (!IsUsedInCompactSpace)
        {
            return 0.0;
        }

        if (_addOnDecoratedBox == null || _addOnDecoratedBox.StyleVariant == InputControlStyleVariant.Borderless)
        {
            return 0.0;
        }

        // 都一样宽
        return _addOnDecoratedBox.InnerBoxBorderThickness.Left;
    }

    #endregion

    #region 实现 FormItem 接口

    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    void IFormItemFeedbackAware.SetFeedbackControl(FormValidateFeedback? value) => NotifySetFeedBackControl(value);

    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifySetFormValue(object? value)
    {
    }

    protected virtual object? NotifyGetFormValue()
    {
        return null;
    }

    protected virtual void NotifyClearFormValue()
    {
    }

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
        if (FormStatus != status)
        {
            SetCurrentValue(FormStatusProperty, status);
        }
    }

    protected virtual void NotifySetFeedBackControl(FormValidateFeedback? value)
    {
        if (!ReferenceEquals(FormFeedback, value))
        {
            FormFeedback = value;
        }
    }

    #endregion

    private void HandleIsDropDownOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // Ignore the change if requested
        if (IgnorePropertyChange)
        {
            IgnorePropertyChange = false;
            return;
        }

        bool oldValue = (bool)e.OldValue!;
        bool newValue = (bool)e.NewValue!;

        if (!newValue && IsPopupPinnedOpen)
        {
            SetDropDownOpenWithoutPropertyHandling(oldValue);
            UpdatePseudoClasses();
            return;
        }

        if (!newValue)
        {
            ClosingDropDown(oldValue);
        }
        else
        {
            OpeningDropDown(oldValue);
        }

        UpdatePseudoClasses();
    }

    private void HandleWindowDeactivated(object? sender, EventArgs e)
    {
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private void IsVisibleChanged(bool isVisible)
    {
        if (!isVisible && IsDropDownOpen)
        {
            SetCurrentValue(IsDropDownOpenProperty, false);
        }
    }

    private void ConfigurePopupPlacement()
    {
        if (Placement == SelectPopupPlacement.BottomEdgeAlignedLeft)
        {
            PopupPlacement = PlacementMode.BottomEdgeAlignedLeft;
        }
        else if (Placement == SelectPopupPlacement.BottomEdgeAlignedRight)
        {
            PopupPlacement = PlacementMode.BottomEdgeAlignedRight;
        }
        else if (Placement == SelectPopupPlacement.TopEdgeAlignedLeft)
        {
            PopupPlacement = PlacementMode.TopEdgeAlignedLeft;
        }
        else if (Placement == SelectPopupPlacement.TopEdgeAlignedRight)
        {
            PopupPlacement = PlacementMode.TopEdgeAlignedRight;
        }
    }

}
