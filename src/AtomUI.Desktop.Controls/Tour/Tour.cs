using AtomUI.Theme.Resources;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using ItemCollection = AtomUI.Collections.ItemCollection;

public enum TourStyleType
{
    Default,
    Primary,
}

public partial class Tour : TemplatedControl, IMotionAwareControl, ISemanticPartCrossRootProvider, IThemeInheritanceSource
{
    #region 公共属性定义

    public static readonly StyledProperty<IEnumerable<ITourStepOption>?> StepsSourceProperty =
        AvaloniaProperty.Register<Tour, IEnumerable<ITourStepOption>?>(nameof(StepsSource));

    public static readonly StyledProperty<IDataTemplate?> ItemTemplateProperty =
        AvaloniaProperty.Register<Tour, IDataTemplate?>(nameof(ItemTemplate));

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Tour>();

    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        AvaloniaProperty.Register<Tour, bool>(nameof(IsArrowVisible), true);

    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        AvaloniaProperty.Register<Tour, bool>(nameof(IsPointAtCenter), false);

    public static readonly StyledProperty<IIconTemplate?> CloseIconProperty =
        AvaloniaProperty.Register<Tour, IIconTemplate?>(nameof(CloseIcon));

    public static readonly StyledProperty<bool> IsDisabledInteractionProperty =
        AvaloniaProperty.Register<Tour, bool>(nameof(IsDisabledInteraction));

    public static readonly StyledProperty<double> GapOffsetXProperty =
        AvaloniaProperty.Register<Tour, double>(nameof(GapOffsetX), 6);

    public static readonly StyledProperty<double> GapOffsetYProperty =
        AvaloniaProperty.Register<Tour, double>(nameof(GapOffsetY), 6);

    public static readonly StyledProperty<double> GapRadiusProperty =
        AvaloniaProperty.Register<Tour, double>(nameof(GapRadius), 2);

    public static readonly StyledProperty<TourPlacementMode> PlacementProperty =
        AvaloniaProperty.Register<Tour, TourPlacementMode>(nameof(Placement));

    public static readonly StyledProperty<bool> IsShowMaskProperty =
        AvaloniaProperty.Register<Tour, bool>(nameof(IsShowMask), true);

    public static readonly StyledProperty<IBrush?> MaskColorProperty =
        AvaloniaProperty.Register<Tour, IBrush?>(nameof(MaskColor));

    public static readonly StyledProperty<TourStyleType> StyleTypeProperty =
        AvaloniaProperty.Register<Tour, TourStyleType>(nameof(StyleType));

    public static readonly StyledProperty<bool> IsScrollIntoViewProperty =
        AvaloniaProperty.Register<Tour, bool>(nameof(IsScrollIntoView), true);

    public static readonly StyledProperty<bool> IsOpenProperty =
        AvaloniaProperty.Register<Tour, bool>(
            nameof(IsOpen),
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<Tour, int> CurrentIndexProperty =
        AvaloniaProperty.RegisterDirect<Tour, int>(
            nameof(CurrentIndex),
            o => o.CurrentIndex,
            (o, v) => o.CurrentIndex = v,
            unsetValue: -1,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<Tour, int> StepCountProperty =
        AvaloniaProperty.RegisterDirect<Tour, int>(nameof(StepCount), o => o.StepCount);

    public static readonly StyledProperty<TourIndicator?> IndicatorProperty =
        AvaloniaProperty.Register<Tour, TourIndicator?>(nameof(Indicator));

    /// <summary>
    /// Gets or sets a value indicating whether the tour popup remains open without requiring
    /// user interaction. Gallery semantic previews pin the popup open this way so the popup
    /// parts can be inspected and highlighted.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<Tour>();

    public IEnumerable<ITourStepOption>? StepsSource
    {
        get => GetValue(StepsSourceProperty);
        set => SetValue(StepsSourceProperty, value);
    }

    [InheritDataTypeFromItems(nameof(StepsSource))]
    public IDataTemplate? ItemTemplate
    {
        get => GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    public bool IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }

    public IIconTemplate? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

    public bool IsDisabledInteraction
    {
        get => GetValue(IsDisabledInteractionProperty);
        set => SetValue(IsDisabledInteractionProperty, value);
    }

    public double GapOffsetX
    {
        get => GetValue(GapOffsetXProperty);
        set => SetValue(GapOffsetXProperty, value);
    }

    public double GapOffsetY
    {
        get => GetValue(GapOffsetYProperty);
        set => SetValue(GapOffsetYProperty, value);
    }

    public double GapRadius
    {
        get => GetValue(GapRadiusProperty);
        set => SetValue(GapRadiusProperty, value);
    }

    public TourPlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public bool IsShowMask
    {
        get => GetValue(IsShowMaskProperty);
        set => SetValue(IsShowMaskProperty, value);
    }

    public IBrush? MaskColor
    {
        get => GetValue(MaskColorProperty);
        set => SetValue(MaskColorProperty, value);
    }

    public TourStyleType StyleType
    {
        get => GetValue(StyleTypeProperty);
        set => SetValue(StyleTypeProperty, value);
    }

    public bool IsScrollIntoView
    {
        get => GetValue(IsScrollIntoViewProperty);
        set => SetValue(IsScrollIntoViewProperty, value);
    }

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private int _currentIndex;

    public int CurrentIndex
    {
        get => _currentIndex;
        private set => SetAndRaise(CurrentIndexProperty, ref _currentIndex, value);
    }

    private int _stepCount;

    public int StepCount
    {
        get => _stepCount;
        private set => SetAndRaise(StepCountProperty, ref _stepCount, value);
    }

    public TourIndicator? Indicator
    {
        get => GetValue(IndicatorProperty);
        set => SetValue(IndicatorProperty, value);
    }

    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    [Content]
    public ItemCollection Steps { get; } = new();

    public ItemCollection CustomActions { get; } = new();

    #endregion

    #region 公共事件定义

    /// <inheritdoc />
    public event EventHandler? CrossRootsChanged;

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<Tour, Rect> TargetClipBoundsProperty =
        AvaloniaProperty.RegisterDirect<Tour, Rect>(nameof(TargetClipBounds),
            o => o.TargetClipBounds,
            (o, v) => o.TargetClipBounds = v);

    internal static readonly DirectProperty<Tour, bool> IsPopupHorizontalFlippedProperty =
        AvaloniaProperty.RegisterDirect<Tour, bool>(nameof(IsPopupHorizontalFlipped),
            o => o.IsPopupHorizontalFlipped,
            (o, v) => o.IsPopupHorizontalFlipped = v);

    internal static readonly DirectProperty<Tour, bool> IsPopupVerticalFlippedProperty =
        AvaloniaProperty.RegisterDirect<Tour, bool>(nameof(IsPopupVerticalFlipped),
            o => o.IsPopupVerticalFlipped,
            (o, v) => o.IsPopupVerticalFlipped = v);

    internal static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        ArrowDecoratedBox.ArrowPositionProperty.AddOwner<Tour>();

    internal static readonly DirectProperty<Tour, TourStyleType> CurrentStyleTypeProperty =
        AvaloniaProperty.RegisterDirect<Tour, TourStyleType>(nameof(CurrentStyleType),
            o => o.CurrentStyleType,
            (o, v) => o.CurrentStyleType = v);

    internal static readonly DirectProperty<Tour, bool> CurrentArrowVisibleProperty =
        AvaloniaProperty.RegisterDirect<Tour, bool>(nameof(CurrentArrowVisible),
            o => o.CurrentArrowVisible,
            (o, v) => o.CurrentArrowVisible = v);

    internal static readonly DirectProperty<Tour, IBrush?> CurrentMaskColorProperty =
        AvaloniaProperty.RegisterDirect<Tour, IBrush?>(nameof(CurrentMaskColor),
            o => o.CurrentMaskColor,
            (o, v) => o.CurrentMaskColor = v);

    private Rect _targetClipBounds;

    internal Rect TargetClipBounds
    {
        get => _targetClipBounds;
        set => SetAndRaise(TargetClipBoundsProperty, ref _targetClipBounds, value);
    }

    private bool _isPopupHorizontalFlipped;

    internal bool IsPopupHorizontalFlipped
    {
        get => _isPopupHorizontalFlipped;
        private set => SetAndRaise(IsPopupHorizontalFlippedProperty, ref _isPopupHorizontalFlipped, value);
    }

    private bool _isPopupVerticalFlipped;

    internal bool IsPopupVerticalFlipped
    {
        get => _isPopupVerticalFlipped;
        private set => SetAndRaise(IsPopupVerticalFlippedProperty, ref _isPopupVerticalFlipped, value);
    }

    internal ArrowPosition ArrowPosition
    {
        get => GetValue(ArrowPositionProperty);
        set => SetValue(ArrowPositionProperty, value);
    }

    private TourStyleType _currentStyleType;

    internal TourStyleType CurrentStyleType
    {
        get => _currentStyleType;
        private set => SetAndRaise(CurrentStyleTypeProperty, ref _currentStyleType, value);
    }

    private bool _currentArrowVisible;

    internal bool CurrentArrowVisible
    {
        get => _currentArrowVisible;
        private set => SetAndRaise(CurrentArrowVisibleProperty, ref _currentArrowVisible, value);
    }

    private IBrush? _currentMaskColor;

    internal IBrush? CurrentMaskColor
    {
        get => _currentMaskColor;
        private set => SetAndRaise(CurrentMaskColorProperty, ref _currentMaskColor, value);
    }

    #endregion

    private bool _ignorePropertyChanged;

    #region 运行时字段

    private bool _isReallyOpened;

    private Popup? _popup;

    private TourLayer? _layer;

    private CompositeDisposable? _maskBindings;

    private TourStepsView? _stepsView;

    private CompositeDisposable? _indicatorDisposables;

    private Control? _scrollBlockedTarget;

    private IDisposable? _popupFlippedBindings;

    private IDisposable? _popupMarginBinding;
    private TourStep? _observedStep;
    private Control? _observedTarget;

    #endregion

    #region 构造与初始化

    static Tour()
    {
        StepsSourceProperty.Changed.AddClassHandler<Tour>((tour, e) => tour.HandleStepsSourcePropertyChanged(e));
    }

    public Tour()
    {
        Steps.CollectionChanged         += HandleItemsViewCollectionChanged;
        CustomActions.CollectionChanged += HandleCustomActionsChanged;
    }

    #endregion

    #region 公共与受保护 API

    /// <summary>
    /// 遮罩承载在 TopLevel VisualLayerManager 的共享 TourLayer 中，属 owner 视觉树之外的
    /// 跨根宿主；Tour 打开时把该层逻辑挂载到自身（语义样式经逻辑树级联命中），语义预览
    /// 经此契约在打开/关闭时跟随刷新。
    /// </summary>
    public IReadOnlyList<Visual> GetCrossRoots()
    {
        return _layer is { } layer && layer.IsVisible
            ? new Visual[] { layer }
            : Array.Empty<Visual>();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Indicator == null)
        {
            SetCurrentValue(IndicatorProperty, new DefaultTourIndicator());
        }
        ConfigureDefaultValues();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == IsOpenProperty)
            {
                if (!_ignorePropertyChanged)
                {
                    if (!change.GetNewValue<bool>() && IsPopupPinnedOpen)
                    {
                        using (BeginIgnoringPropertyChanged())
                        {
                            SetCurrentValue(IsOpenProperty, true);
                        }
                    }
                    else if (change.GetNewValue<bool>())
                    {
                        ShowTour();
                    }
                    else
                    {
                        HideTour();
                    }
                }
            }
            else if (change.Property == IsPopupPinnedOpenProperty &&
                     change.GetNewValue<bool>() &&
                     !IsOpen)
            {
                SetCurrentValue(IsOpenProperty, true);
            }
            else if (change.Property == GapRadiusProperty ||
                     change.Property == GapOffsetXProperty ||
                     change.Property == GapOffsetYProperty)
            {
                CalculateTargetClipBounds();
            }
        }

        if (change.Property == GapOffsetXProperty ||
            change.Property == GapOffsetYProperty)
        {
            ConfigurePopupOffset(GetCurrentPlacement());
        }
        if (change.Property == IsMotionEnabledProperty && _layer != null && _isReallyOpened)
        {
            _layer.SetCurrentValue(TourLayer.IsMotionEnabledProperty, change.GetNewValue<bool>());
        }
        if (change.Property == CurrentIndexProperty)
        {
            if (!_ignorePropertyChanged)
            {
                HandleCurrentStepChanged();
            }
        }

        if (change.Property == IndicatorProperty)
        {
            ConfigureIndicatorBindings();
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Steps.CollectionChanged -= HandleItemsViewCollectionChanged;
        Steps.CollectionChanged += HandleItemsViewCollectionChanged;
        CustomActions.CollectionChanged -= HandleCustomActionsChanged;
        CustomActions.CollectionChanged += HandleCustomActionsChanged;
        StepCount = Steps.Count;
        ConfigureIndicatorBindings();
        if (_stepsView != null)
        {
            _stepsView.CloseRequest -= HandleCloseRequest;
            _stepsView.CloseRequest += HandleCloseRequest;
            _stepsView.NavRequest -= HandleNavRequest;
            _stepsView.NavRequest += HandleNavRequest;
            _stepsView.CustomActions = PrepareCustomActions();
        }
    }

    public void ResetState()
    {
        SetCurrentValue(IsOpenProperty, false);
        SetCurrentValue(CurrentIndexProperty, Steps.Count > 0 ? 0 : -1);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var reopen = _isReallyOpened;
        if (reopen)
        {
            ObserveCurrentTarget(null, null);
            BlockTargetScroll(null);
            _popup?.CloseForLifecycle();
            _isReallyOpened = false;
        }
        _stepsView?.ReleaseTemplateContent();
        _popupMarginBinding?.Dispose();
        _popupMarginBinding = null;
        _popupFlippedBindings?.Dispose();
        _popupFlippedBindings = null;
        if (_popup != null)
        {
            _popup.PositionFlipped -= HandlePositionFlipped;
        }
        base.OnApplyTemplate(e);
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        if (_popup != null)
        {
            // These values flow from a runtime popup back to private owner setters.
            // TemplateBinding has no OneWayToSource mode; a reflection binding cannot access
            // those setters safely under AOT. The bindings belong to this template instance.
            _popupFlippedBindings = new CompositeDisposable(
                this.Bind(IsPopupHorizontalFlippedProperty, _popup.GetObservable(Popup.IsHorizontalFlippedProperty)),
                this.Bind(IsPopupVerticalFlippedProperty, _popup.GetObservable(Popup.IsVerticalFlippedProperty)));
            _popup.PositionFlipped += HandlePositionFlipped;
            // _popup.CustomPlacementCallback = parameters =>
            // {
            //     var topLevel = TopLevel.GetTopLevel(this);
            //     if (topLevel != null)
            //     {
            //         parameters.AnchorRectangle = new Rect(topLevel.DesiredSize);
            //     }
            // };
        }
        if (_stepsView != null)
        {
            _stepsView.CloseRequest -= HandleCloseRequest;
            _stepsView.NavRequest   -= HandleNavRequest;
        }
        _stepsView = e.NameScope.Find<TourStepsView>("StepsView");
        if (_stepsView != null)
        {
            _stepsView.CloseRequest  += HandleCloseRequest;
            _stepsView.NavRequest    += HandleNavRequest;
            _stepsView.CustomActions = PrepareCustomActions();
        }
        if (reopen)
        {
            ShowTour();
        }
    }

    public void ShowTour()
    {
        if (_isReallyOpened || _popup == null || Steps.Count == 0)
        {
            return;
        }

        PrepareTourLayer();
        using (BeginIgnoringPropertyChanged())
        {
            SetCurrentValue(IsOpenProperty, true);
            if (CurrentIndex < 0 || CurrentIndex >= Steps.Count)
            {
                SetCurrentValue(CurrentIndexProperty, 0);
            }
        }
        HandleCurrentStepChanged();
        _popup.IsOpen = true;
        _isReallyOpened = true;
        _layer?.SetCurrentValue(TourLayer.IsMotionEnabledProperty, IsMotionEnabled);
    }

    public void HideTour()
    {
        if (IsPopupPinnedOpen)
        {
            return;
        }

        if (!_isReallyOpened || _popup == null)
        {
            return;
        }
        ObserveCurrentTarget(null, null);
        BlockTargetScroll(null);
        if (_layer != null)
        {
            _layer.SetCurrentValue(TourLayer.IsMotionEnabledProperty, false);
            _layer.IsVisible = false;
            DetachMaskLayer(_layer);
        }
        _popup.IsOpen = false;
        _popupMarginBinding?.Dispose();
        _popupMarginBinding = null;
        using (BeginIgnoringPropertyChanged())
        {
            SetCurrentValue(CurrentIndexProperty, 0);
            SetCurrentValue(IsOpenProperty, false);
            _isReallyOpened =  false;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (IsPopupPinnedOpen || IsOpen)
        {
            ShowTour();
        }
        else
        {
            HideTour();
        }

        if (Steps.Count > 0)
        {
            SetCurrentValue(CurrentIndexProperty, 0);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
     {
         CloseTourForLifecycle();
         base.OnDetachedFromVisualTree(e);

         Steps.CollectionChanged -= HandleItemsViewCollectionChanged;
         CustomActions.CollectionChanged -= HandleCustomActionsChanged;

         _indicatorDisposables?.Dispose();
         _indicatorDisposables = null;

         if (_stepsView != null)
         {
             _stepsView.CloseRequest -= HandleCloseRequest;
             _stepsView.NavRequest -= HandleNavRequest;
         }
     }

    #endregion

    #region 实现 IThemeInheritanceSource 接口

    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

    #region 私有实现

    private void RaiseCrossRootsChanged()
    {
        CrossRootsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void HandleItemsViewCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        StepCount = Steps.Count;
        if (IsInitialized)
        {
            ConfigureDefaultValues();
        }
    }

    private void HandleCustomActionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_stepsView != null)
        {
            _stepsView.CustomActions = PrepareCustomActions();
        }
    }

    private Control[] PrepareCustomActions()
    {
        var actions = new Control[CustomActions.Count];
        var index   = 0;
        foreach (var item in CustomActions)
        {
            var action = (Control)item!;
            if (action is ITourAction tourAction)
            {
                tourAction.NotifyAttached(this);
            }
            actions[index++] = action;
        }

        return actions;
    }

    private void ConfigureDefaultValues()
    {
        foreach (var item in Steps)
        {
            if (item is ITourStepOption stepOption)
            {
                stepOption.IsArrowVisible      ??= IsArrowVisible;
                stepOption.IsPointAtCenter  ??= IsPointAtCenter;
                stepOption.IsShowMask       ??= IsShowMask;
                stepOption.StyleType        ??= StyleType;
                stepOption.Placement        ??= Placement;
                stepOption.IsScrollIntoView ??= IsScrollIntoView;
                stepOption.MaskColor        ??= MaskColor;
            }
        }
    }

    private void ConfigureIndicatorBindings()
    {
        _indicatorDisposables?.Dispose();
        _indicatorDisposables = null;
        if (Indicator is { } indicator)
        {
            // Runtime caller content is prepared at Local priority, outside this
            // template's selector tree. These handles belong to this membership.
            _indicatorDisposables = new CompositeDisposable
            {
                BindUtils.RelayBind(this, StepCountProperty, indicator, TourIndicator.StepCountProperty),
                BindUtils.RelayBind(this, CurrentIndexProperty, indicator, TourIndicator.ActiveIndexProperty),
                BindUtils.RelayBind(this, CurrentStyleTypeProperty, indicator, TourIndicator.StyleTypeProperty),
                BindUtils.RelayBind(this, IsMotionEnabledProperty, indicator, TourIndicator.IsMotionEnabledProperty)
            };
        }
    }

    private void ConfigureArrowPosition(bool isHorizontalFlipped = false, bool isVerticalFlipped = false)
    {
        var requestedPlacement = _popup?.RequestedPlacement;
        if (requestedPlacement == null)
        {
            return;
        }

        ConfigureArrowPosition(requestedPlacement.Value, isHorizontalFlipped, isVerticalFlipped);
    }

    private void ConfigureArrowPosition(
        PlacementMode requestedPlacement,
        bool isHorizontalFlipped = false,
        bool isVerticalFlipped = false)
    {
        var arrowPosition = PopupUtils.CalculateArrowPosition(requestedPlacement, null, null);
        if (arrowPosition.HasValue)
        {
            SetCurrentValue(ArrowPositionProperty,
                ArrowPositionUtils.FlipArrowPosition(arrowPosition.Value, isHorizontalFlipped, isVerticalFlipped));
        }
    }

    private void HandlePositionFlipped(object? sender, PopupFlippedEventArgs args)
    {
        ConfigureArrowPosition(args.HorizontalFlipped, args.VerticalFlipped);
    }

    private void HandleStepsSourcePropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ResetState();
        if (!Steps.IsReadOnly)
        {
            Steps.Clear();
        }
        Steps.SetItemsSource(change.GetNewValue<IEnumerable<ITourStepOption>?>());
    }

    private void HandleCloseRequest(object? sender, EventArgs args)
    {
        HideTour();
    }

    private void HandleNavRequest(object? sender, TourStepNavRequestEventArgs args)
    {
        SetCurrentValue(CurrentIndexProperty, args.Index);
    }

    private void PrepareTourLayer()
    {
        _layer = TourLayer.GetTourLayer(this);
        if (_layer != null)
        {
            _layer.SetCurrentValue(TourLayer.IsMotionEnabledProperty, false);
            AttachMaskLayer(_layer);
            // The shared layer lives outside this ControlTheme's template and outlives a Tour.
            // Its runtime owner holds these bindings only until the mask's logical membership exits.
            _maskBindings?.Dispose();
            _maskBindings = new CompositeDisposable(
                _layer.Bind(TourLayer.BackgroundProperty, this.GetObservable(CurrentMaskColorProperty)),
                _layer.Bind(TourLayer.TargetRegionCornerRadiusProperty, this.GetObservable(GapRadiusProperty)),
                _layer.Bind(TourLayer.TargetRegionProperty, this.GetObservable(TargetClipBoundsProperty)));
        }
    }

    private void AttachMaskLayer(TourLayer layer)
    {
        if (!layer.Classes.Contains(TourSemanticParts.PopupMaskClass))
        {
            layer.Classes.Add(TourSemanticParts.PopupMaskClass);
        }

        if (ReferenceEquals(layer.Parent, this))
        {
            return;
        }

        if (layer.Parent is Tour previousOwner)
        {
            if (ReferenceEquals(previousOwner._layer, layer))
            {
                previousOwner._layer = null;
            }
            previousOwner.DetachMaskLayer(layer);
        }

        // VisualLayerManager.AddLayer 内部把共享层逻辑挂到自身；非 null → 非 null 的
        // 逻辑父切换会抛异常，必须先解除再挂到当前 Tour，owner 作用域的语义样式
        //（">>" Descendant 选择器沿逻辑树匹配）才能跨视觉根命中遮罩。
        if (layer.Parent != null)
        {
            ((ISetLogicalParent)layer).SetParent(null);
        }

        LogicalChildren.Add(layer);
        RaiseCrossRootsChanged();
    }

    private void DetachMaskLayer(TourLayer layer)
    {
        if (!ReferenceEquals(layer.Parent, this))
        {
            return;
        }

        // 共享层"谁打开谁拥有，关闭即释放"：脱离当前 Tour 的逻辑父，
        // 下一个打开的 Tour 可以重新挂载归属。
        _maskBindings?.Dispose();
        _maskBindings = null;
        LogicalChildren.Remove(layer);
        ((ISetLogicalParent)layer).SetParent(null);
        RaiseCrossRootsChanged();
    }

    private void CloseTourForLifecycle()
    {
        ObserveCurrentTarget(null, null);
        // A removed anchor cannot be positioned again. Close the host before
        // retiring the margin binding, whose default restoration can reposition it.
        _popup?.CloseForLifecycle();
        _popupMarginBinding?.Dispose();
        _popupMarginBinding = null;
        BlockTargetScroll(null);
        if (_layer != null)
        {
            _layer.SetCurrentValue(TourLayer.IsMotionEnabledProperty, false);
            _layer.IsVisible = false;
            DetachMaskLayer(_layer);
        }

        using (BeginIgnoringPropertyChanged())
        {
            SetCurrentValue(CurrentIndexProperty, 0);
            SetCurrentValue(IsOpenProperty, false);
            _isReallyOpened = false;
        }
    }

    private void HandleCurrentStepChanged()
    {
        if (CurrentIndex == -1 || _popup == null || !IsOpen)
        {
            return;
        }

        if (CurrentIndex < 0 || CurrentIndex >= Steps.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(CurrentIndex));
        }

        var target = GetCurrentTarget();

        var      step   = Steps[CurrentIndex];
        if (step is ITourStepOption stepOption)
        {
            var stepPlacement = stepOption.Placement ?? Placement;
            var popupPlacement = target == null
                ? GetPopupPlacement(TourPlacementMode.Center)
                : GetPopupPlacement(stepPlacement);

            CurrentStyleType    = stepOption.StyleType ?? StyleType;
            CurrentArrowVisible = stepOption.IsArrowVisible ?? IsArrowVisible;
            if (_layer != null)
            {
                CurrentMaskColor = stepOption.MaskColor ?? MaskColor;
                _layer.IsVisible = stepOption.IsShowMask ?? IsShowMask;
            }

            if (target == null)
            {
                CurrentArrowVisible = false;
            }

            ConfigurePopupOffset(stepPlacement);
            // Unchanged placement inputs need not raise another PositionFlipped event.
            // Preserve the physical popup's current direction when only its step content changes.
            var sameAnchor = _popup.PlacementTarget == target && _popup.RequestedPlacement == popupPlacement;
            ConfigureArrowPosition(popupPlacement,
                sameAnchor && _popup.IsHorizontalFlipped,
                sameAnchor && _popup.IsVerticalFlipped);
            _popup.RequestedPlacement = popupPlacement;
        }

        _popup.PlacementTarget = target;
        ObserveCurrentTarget(step as TourStep, target);
        BlockTargetScroll(target);
        CalculateTargetClipBounds();
    }

    private void ObserveCurrentTarget(TourStep? step, Control? target)
    {
        if (!ReferenceEquals(_observedStep, step))
        {
            if (_observedStep != null)
            {
                _observedStep.PropertyChanged -= HandleCurrentStepPropertyChanged;
            }
            _observedStep = step;
            if (step != null)
            {
                step.PropertyChanged += HandleCurrentStepPropertyChanged;
            }
        }
        if (!ReferenceEquals(_observedTarget, target))
        {
            if (_observedTarget != null)
            {
                _observedTarget.LayoutUpdated -= HandleCurrentTargetLayoutUpdated;
                _observedTarget.DetachedFromVisualTree -= HandleCurrentTargetDetached;
            }
            _observedTarget = target;
            if (target != null)
            {
                target.LayoutUpdated += HandleCurrentTargetLayoutUpdated;
                target.DetachedFromVisualTree += HandleCurrentTargetDetached;
            }
        }
    }

    private void HandleCurrentStepPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (IsOpen && e.Property == TourStep.TargetProperty)
        {
            HandleCurrentStepChanged();
        }
    }

    private void HandleCurrentTargetLayoutUpdated(object? sender, EventArgs e)
    {
        if (IsOpen)
        {
            CalculateTargetClipBounds();
        }
    }

    private void HandleCurrentTargetDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (ReferenceEquals(sender, _observedTarget))
        {
            // Pinned blocks business dismissal, not loss of the current anchor.
            CloseTourForLifecycle();
        }
    }

    private Control? GetCurrentTarget()
    {
        if (CurrentIndex < 0 || CurrentIndex >= Steps.Count)
        {
            return null;
        }
        var      step   = Steps[CurrentIndex];
        Control? target = null;
        if (step is ITourStepOption stepOption)
        {
            target = stepOption.Target;
        }
        return target;
    }

    private TourPlacementMode GetCurrentPlacement()
    {
        if (CurrentIndex >= 0 &&
            CurrentIndex < Steps.Count &&
            Steps[CurrentIndex] is ITourStepOption stepOption)
        {
            return stepOption.Placement ?? Placement;
        }

        return Placement;
    }

    private void CalculateTargetClipBounds()
    {
        if (CurrentIndex < 0 || CurrentIndex >= Steps.Count)
        {
            TargetClipBounds = default;
            return;
        }
        var      step   = Steps[CurrentIndex];
        Control? target = null;
        if (step is ITourStepOption stepOption)
        {
            target = stepOption.Target;
        }
        if (target == null)
        {
            TargetClipBounds = default;
            return;
        }
        // TourLayer 绘制在自身坐标系中，且遮罩挖孔（TargetRegion）与其遮罩几何必须同坐标
        // 空间。层可能宿主在 TopLevel 模板的共享 VisualLayerManager（整窗遮罩），也可能
        // 宿主在舞台内嵌的 VisualLayerManager（对齐上游 getPopupContainer=false 的就地遮罩，
        // 只覆盖舞台区域）。因此挖孔统一优先相对层本身换算；层尚未接入视觉树时退回
        // TopLevel 坐标（层接入后会随 TargetRegion 绑定重新计算）。
        if (_layer is { } layer &&
            target.TranslatePoint(new Point(0, 0), layer) is { } layerOffset)
        {
            TargetClipBounds = new Rect(layerOffset, target.Bounds.Size)
                .Inflate(new Thickness(GapOffsetX, GapOffsetY));
            return;
        }
        var topLevel = TopLevel.GetTopLevel(target);
        if (topLevel != null)
        {
            var offset       = target.TranslatePoint(new Point(0, 0), topLevel) ?? default;
            var targetBounds = new Rect(offset, target.Bounds.Size);
            TargetClipBounds = targetBounds.Inflate(new Thickness(GapOffsetX, GapOffsetY));
        }
    }

    private PlacementMode GetPopupPlacement(TourPlacementMode placement)
    {
        return placement switch
        {
            TourPlacementMode.Left => PlacementMode.Left,
            TourPlacementMode.LeftTop => PlacementMode.LeftEdgeAlignedTop,
            TourPlacementMode.LeftBottom => PlacementMode.LeftEdgeAlignedBottom,
            TourPlacementMode.Right => PlacementMode.Right,
            TourPlacementMode.RightTop => PlacementMode.RightEdgeAlignedTop,
            TourPlacementMode.RightBottom => PlacementMode.RightEdgeAlignedBottom,
            TourPlacementMode.Top => PlacementMode.Top,
            TourPlacementMode.TopLeft => PlacementMode.TopEdgeAlignedLeft,
            TourPlacementMode.TopRight => PlacementMode.TopEdgeAlignedRight,
            TourPlacementMode.Bottom => PlacementMode.Bottom,
            TourPlacementMode.BottomLeft => PlacementMode.BottomEdgeAlignedLeft,
            TourPlacementMode.BottomRight => PlacementMode.BottomEdgeAlignedRight,
            TourPlacementMode.Center => PlacementMode.Center,
            _ => PlacementMode.Bottom
        };
    }

    private void ConfigurePopupOffset(TourPlacementMode placement)
    {
        if (_popup == null)
        {
            return;
        }
        AdjustForGap(placement);
        _popup.HorizontalOffset = 0;
        _popup.VerticalOffset   = 0;
    }

    private void BlockTargetScroll(Control? target)
    {
        if (_scrollBlockedTarget != null)
        {
            _scrollBlockedTarget.RemoveHandler(PointerWheelChangedEvent, HandleTargetPointerWheelChanged);
            _scrollBlockedTarget = null;
        }
        if (target != null)
        {
            target.AddHandler(PointerWheelChangedEvent, HandleTargetPointerWheelChanged, RoutingStrategies.Bubble);
            _scrollBlockedTarget = target;
        }
    }

    private static void HandleTargetPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        e.Handled = true;
    }

    private void AdjustForGap(TourPlacementMode placement)
    {
        Debug.Assert(_popup != null);
        var value = placement switch
        {
            TourPlacementMode.Left => GapOffsetX,
            TourPlacementMode.LeftTop => GapOffsetX,
            TourPlacementMode.LeftBottom => GapOffsetX,
            TourPlacementMode.Right => GapOffsetX,
            TourPlacementMode.RightTop => GapOffsetX,
            TourPlacementMode.RightBottom => GapOffsetX,
            TourPlacementMode.Top => GapOffsetY,
            TourPlacementMode.TopLeft => GapOffsetY,
            TourPlacementMode.TopRight => GapOffsetY,
            TourPlacementMode.Bottom => GapOffsetY,
            TourPlacementMode.BottomLeft => GapOffsetY,
            TourPlacementMode.BottomRight => GapOffsetY,
            _ => 0.0d
        };
        // The resolved placement belongs to the current runtime step/popup. Preserve
        // the existing Local-priority sum while observing the Own input; a root Style
        // setter cannot target this template instance with that priority and state.
        _popupMarginBinding?.Dispose();
        _popupMarginBinding = TokenResourceBinder.CreateControlTokenBinding(
            this, _popup, Popup.MarginToAnchorProperty, TourTokenKind.PopupMarginToAnchor,
            BindingPriority.LocalValue, token => value + (token is double margin ? margin : 0d));
    }

    private IgnorePropertyChangedScope BeginIgnoringPropertyChanged() => new IgnorePropertyChangedScope(this);

    #endregion

    #region 私有类型

    private readonly struct IgnorePropertyChangedScope : IDisposable
     {
         private readonly Tour _owner;

         public IgnorePropertyChangedScope(Tour owner)
         {
             _owner                        = owner;
             _owner._ignorePropertyChanged = true;
         }

         public void Dispose() => _owner._ignorePropertyChanged = false;
     }

    #endregion

}
