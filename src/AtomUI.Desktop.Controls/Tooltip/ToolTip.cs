using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Theme.Algorithms;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(":open")]
public partial class ToolTip : ContentControl,
                               IMotionAwareControl,
                               IArrowAwareShadowMaskInfoProvider
{
    #region 附加属性定义

    public static readonly AttachedProperty<object?> TipProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, object?>("Tip");
    
    public static readonly AttachedProperty<double> TipHostWidthProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, double>("TipHostWidth", double.NaN);

    public static readonly AttachedProperty<PresetColorType?> PresetColorProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, PresetColorType?>("PresetColor");
    
    public static readonly AttachedProperty<Color?> ColorProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, Color?>("Color");
    
    public static readonly AttachedProperty<bool> IsUseOverlayHostProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("IsUseOverlayHost");
    
    public static readonly AttachedProperty<bool> IsArrowVisibleProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("IsArrowVisible", true);
    
    public static readonly AttachedProperty<bool> IsPointAtCenterProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("IsPointAtCenter");
    
    public static readonly AttachedProperty<bool> IsOpenProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("IsOpen", coerce: CoerceIsOpen);

    public static readonly AttachedProperty<PlacementMode> PlacementProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, PlacementMode>("Placement",
            defaultValue: PlacementMode.Top);

    public static readonly AttachedProperty<double> HorizontalOffsetProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, double>("HorizontalOffset");

    public static readonly AttachedProperty<double> VerticalOffsetProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, double>("VerticalOffset");
    
    public static readonly AttachedProperty<int> ShowDelayProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, int>("ShowDelay", 400);

    public static readonly AttachedProperty<int> BetweenShowDelayProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, int>("BetweenShowDelay", 100);

    public static readonly AttachedProperty<bool> ShowOnDisabledProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("ShowOnDisabled", defaultValue: false,
            inherits: true);

    public static readonly AttachedProperty<bool> ServiceEnabledProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("ServiceEnabled", defaultValue: true, inherits: true);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ToolTip>();
    
    public static readonly AttachedProperty<double> MarginToAnchorProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, double>("MarginToAnchor", 4);
    
    public static readonly AttachedProperty<bool> IsCustomShowAndHideProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("IsCustomShowAndHide");
    
    public static readonly AttachedProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, TextWrapping>("TextWrapping", TextWrapping.Wrap);

    public static readonly AttachedProperty<TextTrimming> TextTrimmingProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, TextTrimming>("TextTrimming", TextTrimming.None);

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    #endregion

    #region 路由事件定义

    public static readonly RoutedEvent<CancelRoutedEventArgs> ToolTipOpeningEvent =
        RoutedEvent.Register<ToolTip, CancelRoutedEventArgs>("ToolTipOpening", RoutingStrategies.Direct);

    public static readonly RoutedEvent ToolTipClosingEvent =
        RoutedEvent.Register<ToolTip, RoutedEventArgs>("ToolTipClosing", RoutingStrategies.Direct);

    #endregion

    #region 内部属性定义

    internal static readonly AttachedProperty<ToolTip?> ToolTipProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, ToolTip?>("ToolTip");

    internal static readonly AttachedProperty<bool> IsPopupPinnedOpenProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("IsPopupPinnedOpen");

    private static readonly AttachedProperty<EventHandler<VisualTreeAttachmentEventArgs>?> PendingAttachHandlerProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, EventHandler<VisualTreeAttachmentEventArgs>?>("PendingAttachHandler");
    
    internal static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<ToolTip>();
    
    internal static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        Flyout.ShouldUseOverlayPopupProperty.AddOwner<ToolTip>();
    
    internal TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }
    
    internal bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }
    
    #endregion

    private Popup? _popup;
    private CompositeDisposable? _subscriptions;
    private ArrowDecoratedBox? _arrowDecoratedBox;
    private ContentPresenter? _contentPresenter;
    internal Control? AdornedControl { get; private set; }
    internal event EventHandler? Closed;

    static ToolTip()
    {
        IsOpenProperty.Changed.Subscribe(IsOpenChanged);
        TipProperty.Changed.Subscribe(TipChanged);
        IsPopupPinnedOpenProperty.Changed.Subscribe(IsPopupPinnedOpenChanged);
    }

    #region 附加属性访问器

    public static object? GetTip(Control element)
    {
        return element.GetValue(TipProperty);
    }

    public static void SetTip(Control element, object? value)
    {
        element.SetValue(TipProperty, value);
    }

    public static double GetTipHostWidth(Control element)
    {
        return element.GetValue(TipHostWidthProperty);
    }

    public static void SetTipHostWidth(Control element, double value)
    {
        element.SetValue(TipHostWidthProperty, value);
    }

    public static PresetColorType? GetPresetColor(Control element)
    {
        return element.GetValue(PresetColorProperty);
    }

    public static void SetPresetColor(Control element, PresetColorType color)
    {
        element.SetValue(PresetColorProperty, color);
    }

    public static Color? GetColor(Control element)
    {
        return element.GetValue(ColorProperty);
    }

    public static void SetColor(Control element, Color? color)
    {
        element.SetValue(ColorProperty, color);
    }
        
    public static bool GetIsUseOverlayHost(Control element)
    {
        return element.GetValue(IsUseOverlayHostProperty);
    }

    public static void SetIsUseOverlayHost(Control element, bool value)
    {
        element.SetValue(IsUseOverlayHostProperty, value);
    }
    
    public static bool GetIsArrowVisible(Control element)
    {
        return element.GetValue(IsArrowVisibleProperty);
    }

    public static void SetIsArrowVisible(Control element, bool flag)
    {
        element.SetValue(IsArrowVisibleProperty, flag);
    }

    public static bool GetIsPointAtCenter(Control element)
    {
        return element.GetValue(IsPointAtCenterProperty);
    }

    public static void SetIsPointAtCenter(Control element, bool flag)
    {
        element.SetValue(IsPointAtCenterProperty, flag);
    }
    
    public static bool GetIsOpen(Control element)
    {
        return element.GetValue(IsOpenProperty);
    }

    public static void SetIsOpen(Control element, bool value)
    {
        element.SetValue(IsOpenProperty, value);
    }

    public static PlacementMode GetPlacement(Control element)
    {
        return element.GetValue(PlacementProperty);
    }

    public static void SetPlacement(Control element, PlacementMode value)
    {
        element.SetValue(PlacementProperty, value);
    }

    public static double GetHorizontalOffset(Control element)
    {
        return element.GetValue(HorizontalOffsetProperty);
    }

    public static void SetHorizontalOffset(Control element, double value)
    {
        element.SetValue(HorizontalOffsetProperty, value);
    }

    public static double GetVerticalOffset(Control element)
    {
        return element.GetValue(VerticalOffsetProperty);
    }

    public static void SetVerticalOffset(Control element, double value)
    {
        element.SetValue(VerticalOffsetProperty, value);
    }

    public static int GetShowDelay(Control element)
    {
        return element.GetValue(ShowDelayProperty);
    }

    public static void SetShowDelay(Control element, int value)
    {
        element.SetValue(ShowDelayProperty, value);
    }

    public static int GetBetweenShowDelay(Control element)
    {
        return element.GetValue(BetweenShowDelayProperty);
    }

    public static void SetBetweenShowDelay(Control element, int value)
    {
        element.SetValue(BetweenShowDelayProperty, value);
    }

    public static bool GetShowOnDisabled(Control element)
    {
        return element.GetValue(ShowOnDisabledProperty);
    }

    public static void SetShowOnDisabled(Control element, bool value)
    {
        element.SetValue(ShowOnDisabledProperty, value);
    }

    public static bool GetServiceEnabled(Control element)
    {
        return element.GetValue(ServiceEnabledProperty);
    }

    public static void SetServiceEnabled(Control element, bool value)
    {
        element.SetValue(ServiceEnabledProperty, value);
    }

    public static double GetMarginToAnchor(Control element)
    {
        return element.GetValue(MarginToAnchorProperty);
    }

    public static void SetMarginToAnchor(Control element, double margin)
    {
        element.SetValue(MarginToAnchorProperty, margin);
    }
    
    public static bool GetIsCustomShowAndHide(Control element)
    {
        return element.GetValue(IsCustomShowAndHideProperty);
    }

    public static void SetIsCustomShowAndHide(Control element, bool flag)
    {
        element.SetValue(IsCustomShowAndHideProperty, flag);
    }

    public static TextWrapping GetTextWrapping(Control element)
    {
        return element.GetValue(TextWrappingProperty);
    }

    public static void SetTextWrapping(Control element, TextWrapping value)
    {
        element.SetValue(TextWrappingProperty, value);
    }

    public static TextTrimming GetTextTrimming(Control element)
    {
        return element.GetValue(TextTrimmingProperty);
    }

    public static void SetTextTrimming(Control element, TextTrimming value)
    {
        element.SetValue(TextTrimmingProperty, value);
    }

    #endregion

    #region 路由事件访问器

    public static void AddToolTipOpeningHandler(Control element, EventHandler<CancelRoutedEventArgs> handler) =>
        element.AddHandler(ToolTipOpeningEvent, handler);

    public static void RemoveToolTipOpeningHandler(Control element, EventHandler<CancelRoutedEventArgs> handler) =>
        element.RemoveHandler(ToolTipOpeningEvent, handler);

    public static void AddToolTipClosingHandler(Control element, EventHandler<RoutedEventArgs> handler) =>
        element.AddHandler(ToolTipClosingEvent, handler);

    public static void RemoveToolTipClosingHandler(Control element, EventHandler<RoutedEventArgs> handler) =>
        element.RemoveHandler(ToolTipClosingEvent, handler);

    internal static bool GetIsPopupPinnedOpen(Control element)
    {
        return element.GetValue(IsPopupPinnedOpenProperty);
    }

    internal static void SetIsPopupPinnedOpen(Control element, bool value)
    {
        element.SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    #endregion

    private static void IsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var control = (Control)e.Sender;
        ReconcileOpenState(control);
    }

    private static void TipChanged(AvaloniaPropertyChangedEventArgs e)
    {
        ReconcileOpenState((Control)e.Sender);
    }

    private static void IsPopupPinnedOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var control = (Control)e.Sender;
        if (e.GetNewValue<bool>())
        {
            if (!GetIsOpen(control))
            {
                control.SetCurrentValue(IsOpenProperty, true);
                return;
            }
        }
        else if (GetIsOpen(control))
        {
            var toolTip = control.GetValue(ToolTipProperty);
            control.SetCurrentValue(IsOpenProperty, toolTip?._popup?.IsOpen == true);
            return;
        }

        ReconcileOpenState(control);
    }

    private static bool CoerceIsOpen(AvaloniaObject sender, bool value)
    {
        return !value && sender is Control control && GetIsPopupPinnedOpen(control)
            ? true
            : value;
    }

    /// <summary>
    /// 打开状态调和入口。IsOpen 是声明式的期望打开状态，该方法把期望状态
    /// （IsOpen 为 true、Tip 内容就绪、宿主已挂入 visual tree）与物理弹层状态收敛一致。
    /// 所有影响打开状态的输入（IsOpen 变化、Tip 变化、宿主 attach）都汇聚到该方法，
    /// 不允许出现第二条直接开关 popup 的路径。该方法必须是幂等的。
    /// </summary>
    private static void ReconcileOpenState(Control control)
    {
        var toolTip          = control.GetValue(ToolTipProperty);
        var isPhysicallyOpen = toolTip?._popup?.IsOpen == true;
        var isAttached       = control.IsAttachedToVisualTree();
        var wantOpen         = GetIsOpen(control) && GetTip(control) is not null && isAttached;

        if (!GetIsOpen(control) || wantOpen)
        {
            ClearPendingAttachHandler(control);
        }
        else if (!isAttached)
        {
            // 期望打开但宿主尚未挂入 visual tree：挂一次性订阅，attach 后重新调和
            EnsurePendingAttachHandler(control);
        }

        if (wantOpen == isPhysicallyOpen)
        {
            return;
        }

        if (wantOpen)
        {
            var args = new CancelRoutedEventArgs(ToolTipOpeningEvent);
            control.RaiseEvent(args);
            if (args.Cancel)
            {
                control.SetCurrentValue(IsOpenProperty, false);
                return;
            }

            var tip = GetTip(control)!;
            if (toolTip == null || (tip != toolTip && tip != toolTip.Content))
            {
                toolTip?.Close();
                toolTip = tip as ToolTip ?? new ToolTip() { Content = tip };
                control.SetValue(ToolTipProperty, toolTip);
            }

            toolTip.AdornedControl = control;
            toolTip.Open(control);
        }
        else if (toolTip is not null)
        {
            // 只关闭物理弹层；IsOpen 仍为 true（Tip 未就绪或宿主已卸载）时保留期望状态，
            // 条件满足后由调和流程重新打开。
            if (GetIsPopupPinnedOpen(control))
            {
                toolTip.CloseForLifecycle();
            }
            else
            {
                toolTip.Close();
            }
        }
    }

    private static void EnsurePendingAttachHandler(Control control)
    {
        if (control.GetValue(PendingAttachHandlerProperty) is not null)
        {
            return;
        }

        EventHandler<VisualTreeAttachmentEventArgs> handler = (s, _) =>
        {
            if (s is Control attachedControl)
            {
                ClearPendingAttachHandler(attachedControl);
                ReconcileOpenState(attachedControl);
            }
        };
        control.SetValue(PendingAttachHandlerProperty, handler);
        control.AttachedToVisualTree += handler;
    }

    private static void ClearPendingAttachHandler(Control control)
    {
        var handler = control.GetValue(PendingAttachHandlerProperty);
        if (handler is not null)
        {
            control.AttachedToVisualTree -= handler;
            control.ClearValue(PendingAttachHandlerProperty);
        }
    }

    private void Open(Control control)
    {
        Close();
        ApplyTemplate();
        
        if (_popup is null)
        {
            _popup = new Popup();
            _popup.Child = this;
            _popup.TakesFocusFromNativeControl = false;
            _popup.WindowManagerAddShadowHint = false;

            _popup.Opened                       += HandlePopupOpened;
            _popup.Closed                       += HandlePopupClosed;
            _popup.PositionFlipped              += HandlePositionFlipped;
        }

        _subscriptions = new CompositeDisposable([
            Bind(ShouldUseOverlayPopupProperty, control.GetBindingObservable(IsUseOverlayHostProperty)),
            _popup.Bind(Popup.ShouldUseOverlayLayerProperty, this.GetObservable(ShouldUseOverlayPopupProperty)),
            _popup.Bind(Popup.MotionDurationProperty, this.GetObservable(MotionDurationProperty)),
            _popup.Bind(Popup.IsMotionEnabledProperty, this.GetObservable(IsMotionEnabledProperty)),
            _popup.Bind(Popup.HorizontalOffsetProperty, ResolveValueSource(control, HorizontalOffsetProperty).GetBindingObservable(HorizontalOffsetProperty)),
            _popup.Bind(Popup.VerticalOffsetProperty, ResolveValueSource(control, VerticalOffsetProperty).GetBindingObservable(VerticalOffsetProperty)),
            _popup.Bind(Popup.RequestedPlacementProperty, ResolveValueSource(control, PlacementProperty).GetBindingObservable(PlacementProperty, v => (PlacementMode?)v)),
            _popup.Bind(Popup.MarginToAnchorProperty, ResolveValueSource(control, MarginToAnchorProperty).GetBindingObservable(MarginToAnchorProperty)),
            _popup.Bind(Popup.IsPointAtCenterProperty, ResolveValueSource(control, IsPointAtCenterProperty).GetBindingObservable(IsPointAtCenterProperty)),
            _popup.Bind(Popup.IsPopupPinnedOpenProperty, control.GetBindingObservable(IsPopupPinnedOpenProperty)),
        ]);

        _popup.PlacementTarget = control;
        _popup.SetPopupParent(control);

        if (_arrowDecoratedBox is not null)
        {
            SetupArrowDecoratedBox(control);
        }
        else
        {
            TemplateApplied += DeferSetupArrowDecoratedBox;
        }

        ConfigureMotion(_popup, GetToolTipValue(control, PlacementProperty));
        _popup.IsOpen = true;
    }

    /// <summary>
    /// 呈现类附加属性的取值来源解析：Tip 直接传入 ToolTip 实例时，实例自身显式设置
    /// （IsSet）的值优先于宿主控件；未设置的回落到宿主。这让 ToolTip 实例成为完整
    /// 定制面，而不需要宿主侧新增任何配置语言。
    /// </summary>
    private AvaloniaObject ResolveValueSource(Control host, AvaloniaProperty property)
    {
        return IsSet(property) ? this : host;
    }

    private T GetToolTipValue<T>(Control host, AttachedProperty<T> property)
    {
        return IsSet(property) ? GetValue(property) : host.GetValue(property);
    }

    private void ConfigureMotion(Popup popup, PlacementMode placement)
    {
        (popup.OpenMotion, popup.CloseMotion) = PopupUtils.CreateMotionForPlacement(placement);
    }

    private void Close()
    {
        if (AdornedControl is { } adornedControl
            && GetIsOpen(adornedControl))
        {
            var args = new RoutedEventArgs(ToolTipClosingEvent);
            adornedControl.RaiseEvent(args);
        }
        
        if (_popup != null)
        {
            _popup.IsOpen = false;
        }
        else
        {
            _subscriptions?.Dispose();
        }
    }

    private void CloseForLifecycle()
    {
        if (_popup != null)
        {
            _popup.CloseForLifecycle();
        }
        else
        {
            _subscriptions?.Dispose();
        }
    }

    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        if (AdornedControl is { } adornedControl
            && GetIsOpen(adornedControl))
        {
            // Closed 可能在宿主 DetachedFromVisualTree 过程中同步触发，此时 PresentationSource 尚未清空，
            // 无法可靠区分卸载语义；推迟到当前 detach/attach 流程结束后按实际挂载状态收敛期望状态。
            Dispatcher.UIThread.Post(() => ConvergeIsOpenAfterPopupClosed(adornedControl), DispatcherPriority.Background);
        }

        UpdatePseudoClasses(false);
        Closed?.Invoke(this, EventArgs.Empty);
        if (sender is Popup popup)
        {
            popup.SetPopupParent(null);
            popup.PlacementTarget = null;
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }

    private void ConvergeIsOpenAfterPopupClosed(Control adornedControl)
    {
        if (!GetIsOpen(adornedControl) || _popup?.IsOpen == true)
        {
            // 期望状态已被调和流程收敛，或弹层已重新打开
            return;
        }

        if (adornedControl.IsAttachedToVisualTree())
        {
            // 弹层被宿主卸载以外的原因关闭：期望状态回写为关闭
            adornedControl.SetCurrentValue(IsOpenProperty, false);
        }
        else
        {
            // 宿主卸载触发的生命周期关闭：保留 IsOpen 期望状态，重新挂入后由调和流程重开
            EnsurePendingAttachHandler(adornedControl);
        }
    }

    private void HandlePositionFlipped(object? sender, PopupFlippedEventArgs args)
    {
        if (sender is Popup popup && popup.PlacementTarget != null)
        {
            SetupArrowPosition(GetToolTipValue(popup.PlacementTarget, PlacementProperty), args.HorizontalFlipped, args.VerticalFlipped);
        }
    }

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        UpdatePseudoClasses(true);
    }

    private void UpdatePseudoClasses(bool newValue)
    {
        PseudoClasses.Set(ToolTipPseudoClass.Open, newValue);
    }

    CornerRadius IShadowMaskInfoProvider.GetMaskCornerRadius()
    {
        return ResolveArrowDecoratedBox()?.GetMaskCornerRadius() ?? new CornerRadius(0);
    }

    Rect IShadowMaskInfoProvider.GetMaskBounds()
    {
        return ResolveArrowDecoratedBox()?.GetMaskBounds() ?? new Rect(Bounds.Size);
    }

    IBrush? IShadowMaskInfoProvider.GetMaskBackground()
    {
        return Background;
    }

    ArrowPosition IArrowAwareShadowMaskInfoProvider.GetArrowPosition()
    {
        return ResolveArrowDecoratedBox()?.ArrowPosition ?? default;
    }

    bool IArrowAwareShadowMaskInfoProvider.IsArrowVisible()
    {
        // 箭头部件不可用时按无箭头报告，弹层据此降级阴影与定位
        return ResolveArrowDecoratedBox()?.IsArrowVisible ?? false;
    }

    void IArrowAwareShadowMaskInfoProvider.SetArrowOpacity(double opacity)
    {
        if (ResolveArrowDecoratedBox() is { } arrowDecoratedBox)
        {
            arrowDecoratedBox.ArrowOpacity = opacity;
        }
    }

    Rect IArrowAwareShadowMaskInfoProvider.GetArrowIndicatorBounds()
    {
        return ResolveArrowDecoratedBox()?.ArrowIndicatorBounds ?? default;
    }

    Rect IArrowAwareShadowMaskInfoProvider.GetArrowIndicatorLayoutBounds()
    {
        return ResolveArrowDecoratedBox()?.ArrowIndicatorLayoutBounds ?? default;
    }

    AbstractArrowDecoratedBox? IArrowAwareShadowMaskInfoProvider.GetArrowDecoratedBox()
    {
        return ResolveArrowDecoratedBox();
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _arrowDecoratedBox = e.NameScope.Find<ArrowDecoratedBox>(ArrowDecoratedBox.ArrowDecoratorPart);
        _contentPresenter  = e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");
    }
    
    private void DeferSetupArrowDecoratedBox(object? sender, TemplateAppliedEventArgs args)
    {
        if (_arrowDecoratedBox is null)
        {
            // 本次应用的模板仍未提供箭头部件：保持订阅，等待后续模板应用再完成箭头设置
            return;
        }
        TemplateApplied -= DeferSetupArrowDecoratedBox;
        Debug.Assert(_popup != null && _popup.PlacementTarget != null);
        SetupArrowDecoratedBox(_popup.PlacementTarget);
    }
    
    private void SetupArrowDecoratedBox(Control control)
    {
        if (_arrowDecoratedBox is not null)
        {
            SetToolTipColor(control);
            if (_contentPresenter != null)
            {
                _contentPresenter.Width = GetToolTipValue(control, TipHostWidthProperty);
                _subscriptions?.Add(_contentPresenter.Bind(ContentPresenter.TextWrappingProperty,
                    ResolveValueSource(control, TextWrappingProperty).GetBindingObservable(TextWrappingProperty)));
                _subscriptions?.Add(_contentPresenter.Bind(ContentPresenter.TextTrimmingProperty,
                    ResolveValueSource(control, TextTrimmingProperty).GetBindingObservable(TextTrimmingProperty)));
            }

            _arrowDecoratedBox.Bind(ArrowDecoratedBox.IsArrowVisibleProperty,
                ResolveValueSource(control, IsArrowVisibleProperty).GetBindingObservable(IsArrowVisibleProperty, flag =>
                {
                    // 有些条件下是不能开启箭头指针的
                    if (flag && _popup is not null)
                    {
                        return PopupUtils.CanEnabledArrow(GetToolTipValue(control, PlacementProperty));
                    }

                    return flag;
                }));
            if (_popup is not null)
            {
                SetupArrowPosition(GetToolTipValue(control, PlacementProperty), false, false);
            }
        }
    }

    private ArrowDecoratedBox? ResolveArrowDecoratedBox()
    {
        if (_arrowDecoratedBox is null)
        {
            // 尽力在已挂树/已样式化时立即构建模板；模板未就绪或主题未提供
            // PART_ArrowDecorator 部件时返回 null，由调用方按无箭头降级处理
            ApplyTemplate();
        }

        return _arrowDecoratedBox;
    }
    
    private void SetupArrowPosition(PlacementMode placement, bool isHorizontalFlipped, bool isVerticalFlipped, PopupAnchor? anchor = null, PopupGravity? gravity = null)
    {
        var arrowPosition = PopupUtils.CalculateArrowPosition(placement, anchor, gravity);
        if (_arrowDecoratedBox is not null && arrowPosition is not null)
        {
            _arrowDecoratedBox.ArrowPosition = ArrowPositionUtils.FlipArrowPosition(arrowPosition.Value, isHorizontalFlipped, isVerticalFlipped);
        }
    }
    
    private void SetToolTipColor(Control control)
    {
        // Preset 优先级高
        if (_arrowDecoratedBox is not null)
        {
            var presetColorType = GetToolTipValue(control, PresetColorProperty);
            var color           = GetToolTipValue(control, ColorProperty);
            if (presetColorType is not null)
            {
                var presetColor = PresetPrimaryColor.GetColor(presetColorType.Value);
                SetBackgroundColorIfChanged(presetColor.Color());
            }
            else if (color is not null)
            {
                SetBackgroundColorIfChanged(color.Value);
            }
        }
    }

    private void SetBackgroundColorIfChanged(Color color)
    {
        if (Background is ISolidColorBrush { Opacity: 1.0 } solidColorBrush &&
            solidColorBrush.Color == color)
        {
            return;
        }

        Background = new SolidColorBrush(color);
        InvalidateVisual();
    }
}
