using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AtomUIFlyout = AtomUI.Desktop.Controls.Flyout;
using AvaloniaButton = Avalonia.Controls.Button;

public enum ColorPickerValueSyncMode
{
    Immediate,
    OnCompleted
}

public abstract class AbstractColorPicker : AvaloniaButton,
                                            ICustomizableSizeTypeAware,
                                            IMotionAwareControl,
                                            ICompactSpaceAware,
                                            IFormItemAware,
                                            IInputControlStatusAware,
                                            IInputControlStyleVariantAware
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsNeedConfirmProperty =
        AvaloniaProperty.Register<AbstractColorPicker, bool>(nameof(IsNeedConfirm));

    public static readonly StyledProperty<ColorFormat> FormatProperty =
        AbstractColorPickerView.FormatProperty.AddOwner<AbstractColorPicker>();

    public static readonly StyledProperty<FlyoutTriggerType> TriggerTypeProperty =
        AvaloniaProperty.Register<AbstractColorPicker, FlyoutTriggerType>(nameof(TriggerType), FlyoutTriggerType.Click);

    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<AbstractColorPicker>();

    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        AtomUIFlyout.IsPointAtCenterProperty.AddOwner<AbstractColorPicker>();

    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        Popup.PlacementProperty.AddOwner<AbstractColorPicker>();

    public static readonly StyledProperty<PopupGravity> PlacementGravityProperty =
        Popup.PlacementGravityProperty.AddOwner<AbstractColorPicker>();

    public static readonly StyledProperty<PopupAnchor> PlacementAnchorProperty =
        Popup.PlacementAnchorProperty.AddOwner<AbstractColorPicker>();

    public static readonly StyledProperty<double> MarginToAnchorProperty =
        Popup.MarginToAnchorProperty.AddOwner<AbstractColorPicker>();

    public static readonly StyledProperty<bool> IsAlphaEnabledProperty =
        AbstractColorPickerView.IsAlphaEnabledProperty.AddOwner<AbstractColorPicker>();

    public static readonly StyledProperty<bool> IsFormatEnabledProperty =
        AbstractColorPickerView.IsFormatEnabledProperty.AddOwner<AbstractColorPicker>();

    public static readonly StyledProperty<bool> IsTextVisibleProperty =
        AvaloniaProperty.Register<AbstractColorPicker, bool>(nameof(IsTextVisible));

    public static readonly StyledProperty<bool> IsClearEnabledProperty =
        AbstractColorPickerView.IsClearEnabledProperty.AddOwner<AbstractColorPicker>();

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractColorPicker>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractColorPicker>();

    public static readonly StyledProperty<string> EmptyColorTextProperty =
        AvaloniaProperty.Register<AbstractColorPicker, string>(nameof(EmptyColorText), string.Empty);

    public static readonly StyledProperty<bool> IsPaletteGroupEnabledProperty =
        AvaloniaProperty.Register<AbstractColorPicker, bool>(nameof(IsPaletteGroupEnabled));

    public static readonly StyledProperty<List<ColorPickerPalette>?> PaletteGroupProperty =
        ColorPickerPaletteGroup.PaletteGroupProperty.AddOwner<AbstractColorPicker>();

    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<AbstractColorPicker>();

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<AbstractColorPicker>();

    public static readonly StyledProperty<int> MouseEnterDelayProperty =
        AvaloniaProperty.Register<AbstractColorPicker, int>(nameof(MouseEnterDelay), 200);

    public static readonly StyledProperty<int> MouseLeaveDelayProperty =
        AvaloniaProperty.Register<AbstractColorPicker, int>(nameof(MouseLeaveDelay), 200);

    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<AbstractColorPicker, bool>(nameof(ShouldUseOverlayPopup), true);

    /// <summary>
    /// Gets or sets whether the picker popup remains open while its placement target is available.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<AbstractColorPicker>();

    public bool IsNeedConfirm
    {
        get => GetValue(IsNeedConfirmProperty);
        set => SetValue(IsNeedConfirmProperty, value);
    }

    public ColorFormat Format
    {
        get => GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public FlyoutTriggerType TriggerType
    {
        get => GetValue(TriggerTypeProperty);
        set => SetValue(TriggerTypeProperty, value);
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

    public PlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public PopupGravity PlacementGravity
    {
        get => GetValue(PlacementGravityProperty);
        set => SetValue(PlacementGravityProperty, value);
    }

    public PopupAnchor PlacementAnchor
    {
        get => GetValue(PlacementAnchorProperty);
        set => SetValue(PlacementAnchorProperty, value);
    }

    public double MarginToAnchor
    {
        get => GetValue(MarginToAnchorProperty);
        set => SetValue(MarginToAnchorProperty, value);
    }

    public bool IsAlphaEnabled
    {
        get => GetValue(IsAlphaEnabledProperty);
        set => SetValue(IsAlphaEnabledProperty, value);
    }

    public bool IsFormatEnabled
    {
        get => GetValue(IsFormatEnabledProperty);
        set => SetValue(IsFormatEnabledProperty, value);
    }

    public bool IsTextVisible
    {
        get => GetValue(IsTextVisibleProperty);
        set => SetValue(IsTextVisibleProperty, value);
    }

    public bool IsClearEnabled
    {
        get => GetValue(IsClearEnabledProperty);
        set => SetValue(IsClearEnabledProperty, value);
    }

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public string EmptyColorText
    {
        get => GetValue(EmptyColorTextProperty);
        set => SetValue(EmptyColorTextProperty, value);
    }

    public bool IsPaletteGroupEnabled
    {
        get => GetValue(IsPaletteGroupEnabledProperty);
        set => SetValue(IsPaletteGroupEnabledProperty, value);
    }

    public List<ColorPickerPalette>? PaletteGroup
    {
        get => GetValue(PaletteGroupProperty);
        set => SetValue(PaletteGroupProperty, value);
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

    public int MouseEnterDelay
    {
        get => GetValue(MouseEnterDelayProperty);
        set => SetValue(MouseEnterDelayProperty, value);
    }

    public int MouseLeaveDelay
    {
        get => GetValue(MouseLeaveDelayProperty);
        set => SetValue(MouseLeaveDelayProperty, value);
    }

    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the picker popup remains open while its placement target is available.
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<AbstractColorPicker, IBrush?> ColorBlockBackgroundProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPicker, IBrush?>(
            nameof(ColorBlockBackground),
            o => o.ColorBlockBackground,
            (o, v) => o.ColorBlockBackground = v);

    internal static readonly DirectProperty<AbstractColorPicker, CornerRadius> EffectiveCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPicker, CornerRadius>(nameof(EffectiveCornerRadius),
            o => o.EffectiveCornerRadius,
            (o, v) => o.EffectiveCornerRadius = v);

    internal static readonly DirectProperty<AbstractColorPicker, double> ColorBlockSizeProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPicker, double>(nameof(ColorBlockSize),
            o => o.ColorBlockSize,
            (o, v) => o.ColorBlockSize = v);

    internal static readonly StyledProperty<Thickness> TriggerPaddingProperty =
        AvaloniaProperty.Register<AbstractColorPicker, Thickness>(nameof(TriggerPadding));

    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty =
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<AbstractColorPicker>();

    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty =
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<AbstractColorPicker>();

    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty =
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<AbstractColorPicker>();

    internal static readonly StyledProperty<bool> IsPickerOpenProperty =
        AvaloniaProperty.Register<AbstractColorPicker, bool>(nameof(IsPickerOpen), coerce: CoerceIsPickerOpen);

    internal static readonly StyledProperty<Control?> PickerPresenterProperty =
        AvaloniaProperty.Register<AbstractColorPicker, Control?>(nameof(PickerPresenter));

    internal static readonly DirectProperty<AbstractColorPicker, bool> IsArrowVisibleEffectiveProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPicker, bool>(nameof(IsArrowVisibleEffective),
            o => o.IsArrowVisibleEffective,
            (o, v) => o.IsArrowVisibleEffective = v);

    internal static readonly DirectProperty<AbstractColorPicker, bool> IsPopupHorizontalFlippedProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPicker, bool>(nameof(IsPopupHorizontalFlipped),
            o => o.IsPopupHorizontalFlipped,
            (o, v) => o.IsPopupHorizontalFlipped = v);

    internal static readonly DirectProperty<AbstractColorPicker, bool> IsPopupVerticalFlippedProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPicker, bool>(nameof(IsPopupVerticalFlipped),
            o => o.IsPopupVerticalFlipped,
            (o, v) => o.IsPopupVerticalFlipped = v);

    internal static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        ArrowDecoratedBox.ArrowPositionProperty.AddOwner<AbstractColorPicker>();

    internal static readonly DirectProperty<AbstractColorPicker, bool> IsLightDismissEnabledProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPicker, bool>(nameof(IsLightDismissEnabled),
            o => o.IsLightDismissEnabled,
            (o, v) => o.IsLightDismissEnabled = v);

    private IBrush? _colorBlockBackground;

    internal IBrush? ColorBlockBackground
    {
        get => _colorBlockBackground;
        set => SetAndRaise(ColorBlockBackgroundProperty, ref _colorBlockBackground, value);
    }

    private CornerRadius _effectiveCornerRadius;

    internal CornerRadius EffectiveCornerRadius
    {
        get => _effectiveCornerRadius;
        set => SetAndRaise(EffectiveCornerRadiusProperty, ref _effectiveCornerRadius, value);
    }

    private double _colorBlockSize = double.NaN;

    internal double ColorBlockSize
    {
        get => _colorBlockSize;
        private set => SetAndRaise(ColorBlockSizeProperty, ref _colorBlockSize, value);
    }

    internal Thickness TriggerPadding
    {
        get => GetValue(TriggerPaddingProperty);
        set => SetValue(TriggerPaddingProperty, value);
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

    internal bool IsPickerOpen
    {
        get => GetValue(IsPickerOpenProperty);
        set => SetValue(IsPickerOpenProperty, value);
    }

    internal Control? PickerPresenter
    {
        get => GetValue(PickerPresenterProperty);
        set => SetValue(PickerPresenterProperty, value);
    }

    private bool _isArrowVisibleEffective;

    internal bool IsArrowVisibleEffective
    {
        get => _isArrowVisibleEffective;
        private set => SetAndRaise(IsArrowVisibleEffectiveProperty, ref _isArrowVisibleEffective, value);
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

    private bool _isLightDismissEnabled;

    internal bool IsLightDismissEnabled
    {
        get => _isLightDismissEnabled;
        private set => SetAndRaise(IsLightDismissEnabledProperty, ref _isLightDismissEnabled, value);
    }

    protected void SetColorBlockBackgroundBrushIfChanged(IBrush? brush)
    {
        if (ReferenceEquals(ColorBlockBackground, brush))
        {
            return;
        }

        SetCurrentValue(ColorBlockBackgroundProperty, brush);
    }

    protected void SetColorBlockBackgroundColorIfChanged(Color color)
    {
        if (ColorBlockBackground is ISolidColorBrush { Opacity: 1.0 } solidColorBrush &&
            solidColorBrush.Color == color)
        {
            return;
        }

        SetCurrentValue(ColorBlockBackgroundProperty, new SolidColorBrush(color));
    }

    #endregion

    private TopLevel? _deactivationTopLevel;
    private IDisposable? _deactivationSubscription;
    private Popup? _popup;
    private CompositeDisposable? _triggerSubscriptions;
    private DispatcherTimer? _mouseEnterDelayTimer;
    private DispatcherTimer? _mouseLeaveDelayTimer;
    private IDisposable? _popupPointerSubscription;
    private TopLevel? _registeredTopLevel;
    private bool _isPickerShowing;
    private int _popupLifecycleCloseDepth;
    // IsAttachedToVisualTree() still reports true inside OnDetachedFromVisualTree, so track attachment explicitly.
    private bool _isAttachedToVisualTree;

    static AbstractColorPicker()
    {
        AffectsMeasure<AbstractColorPicker>(IsTextVisibleProperty,
            FormatProperty,
            ColorBlockBackgroundProperty,
            ColorBlockSizeProperty,
            TriggerPaddingProperty);
        IsPickerOpenProperty.Changed.AddClassHandler<AbstractColorPicker>((picker, args) => picker.HandleIsPickerOpenChanged(args));
        TriggerTypeProperty.Changed.AddClassHandler<AbstractColorPicker>((picker, _) => picker.SetupTriggerHandler());
        InputElement.PointerPressedEvent.AddClassHandler<AbstractColorPicker>(
            (picker, args) => picker.HandleClickModePointerPressed(args),
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
    }

    public AbstractColorPicker()
    {
    }

    #region TriggerType 实现

    private void SetupTriggerHandler()
    {
        _triggerSubscriptions?.Dispose();
        StopMouseEnterTimer();
        StopMouseLeaveTimer();
        UnregisterRootPointerHandler();
        UnsubscribeFromPopupPointer();

        IsLightDismissEnabled = TriggerType == FlyoutTriggerType.Click;
        ApplyPopupTriggerSettings();

        _triggerSubscriptions = null;

        switch (TriggerType)
        {
            case FlyoutTriggerType.Hover:
                _triggerSubscriptions = new CompositeDisposable();
                SetupHoverTrigger();
                if (IsPickerOpen)
                {
                    SubscribeToPopupPointer();
                }
                break;
            case FlyoutTriggerType.Click:
                break;
            case FlyoutTriggerType.Focus:
                _triggerSubscriptions = new CompositeDisposable();
                SetupFocusTrigger();
                if (IsPickerOpen)
                {
                    RegisterRootPointerHandler();
                }
                break;
        }
    }

    private void ApplyPopupTriggerSettings()
    {
        if (_popup == null)
        {
            return;
        }
        _popup.OverlayDismissEventPassThrough = TriggerType != FlyoutTriggerType.Click;
        _popup.SetCurrentValue(
            Popup.IsLightDismissEnabledProperty,
            IsLightDismissEnabled);
    }

    #region Hover trigger

    private void SetupHoverTrigger()
    {
        _triggerSubscriptions!.Add(this.GetObservable(IsPointerOverProperty).Subscribe(isPointerOver =>
        {
            if (IsEnabled && IsVisible)
            {
                HandleAnchorHover(isPointerOver);
            }
        }));
    }

    private void HandleAnchorHover(bool isPointerOver)
    {
        if (isPointerOver)
        {
            ShowPicker();
        }
        else
        {
            StopMouseEnterTimer();
            if (IsPickerOpen && _popup?.Child is InputElement popupChild && popupChild.IsPointerOver)
            {
                return;
            }
            StartMouseLeaveTimer();
        }
    }

    private void SubscribeToPopupPointer()
    {
        _popupPointerSubscription?.Dispose();
        _popupPointerSubscription = null;

        // IInputManager.Process is app-wide; subscribing while detached (e.g. TriggerType changed with
        // IsPickerOpen already true) would never be released and would root this picker.
        if (!_isAttachedToVisualTree)
        {
            return;
        }

        var inputManager = AvaloniaLocator.Current.GetService<IInputManager>();
        if (inputManager != null)
        {
            _popupPointerSubscription = inputManager.Process.Subscribe(HandleGlobalPointerMove);
        }
    }

    private void UnsubscribeFromPopupPointer()
    {
        _popupPointerSubscription?.Dispose();
        _popupPointerSubscription = null;
    }

    private void HandleGlobalPointerMove(RawInputEventArgs e)
    {
        if (e is not RawPointerEventArgs pe || pe.Type != RawPointerEventType.Move)
        {
            return;
        }

        var hitElement = pe.GetInputHitTestResult().element;
        if (hitElement is not Visual visual)
        {
            return;
        }

        if (IsVisualInPickerScope(visual))
        {
            StopMouseLeaveTimer();
        }
        else if (!IsPointerOver)
        {
            if (_mouseLeaveDelayTimer == null && IsPickerOpen)
            {
                StartMouseLeaveTimer();
            }
        }
    }

    #endregion

    #region Click trigger

    private void HandleClickModePointerPressed(PointerPressedEventArgs e)
    {
        if (TriggerType != FlyoutTriggerType.Click)
        {
            return;
        }

        if (!IsEnabled || !IsVisible)
        {
            return;
        }

        if (e.Source is Visual source && IsSourceInsidePickerPopup(source))
        {
            return;
        }

        if (IsPickerOpen)
        {
            HidePicker(immediately: true);
        }
        else
        {
            ShowPicker(immediately: true);
        }
    }

    private bool IsSourceInsidePickerPopup(Visual visual)
    {
        if (visual == this || this.IsVisualAncestorOf(visual))
        {
            return false;
        }
        return IsVisualInPickerScope(visual);
    }

    #endregion

    #region Focus trigger

    private void SetupFocusTrigger()
    {
        GotFocus += HandleFocusModeGotFocus;
        LostFocus += HandleFocusModeLostFocus;
        AddHandler(InputElement.PointerPressedEvent, HandleFocusModeAnchorPointerPressed,
            handledEventsToo: true);
        _triggerSubscriptions!.Add(Disposable.Create(() =>
        {
            GotFocus -= HandleFocusModeGotFocus;
            LostFocus -= HandleFocusModeLostFocus;
            RemoveHandler(InputElement.PointerPressedEvent, HandleFocusModeAnchorPointerPressed);
            UnregisterRootPointerHandler();
        }));
    }

    private void HandleFocusModeGotFocus(object? sender, FocusChangedEventArgs e)
    {
        TryShowIfClosed();
    }

    private void HandleFocusModeAnchorPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        TryShowIfClosed();
    }

    private void TryShowIfClosed()
    {
        if (IsEnabled && IsVisible && !IsPickerOpen)
        {
            ShowPicker(immediately: true);
        }
    }

    private void HandleFocusModeLostFocus(object? sender, FocusChangedEventArgs e)
    {
        if (!IsPickerOpen || _isPickerShowing)
        {
            return;
        }

        Dispatcher.Post(() =>
        {
            if (IsPickerOpen && !_isPickerShowing && !IsFocusWithinPickerScope())
            {
                HidePicker(immediately: true);
            }
        });
    }

    #endregion

    #region Root pointer handler for Focus mode

    private void RegisterRootPointerHandler()
    {
        UnregisterRootPointerHandler();
        _registeredTopLevel = TopLevel.GetTopLevel(this);
        _registeredTopLevel?.AddHandler(PointerPressedEvent, HandleFocusModeRootPointerPressed,
            RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    private void UnregisterRootPointerHandler()
    {
        if (_registeredTopLevel != null)
        {
            _registeredTopLevel.RemoveHandler(PointerPressedEvent, HandleFocusModeRootPointerPressed);
            _registeredTopLevel = null;
        }
    }

    private void HandleFocusModeRootPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsPickerOpen)
        {
            return;
        }

        if (e.Source is Visual source && !IsVisualInPickerScope(source))
        {
            HidePicker(immediately: true);
        }
    }

    #endregion

    #region Timer helpers

    private void StartMouseEnterTimer()
    {
        StopMouseEnterTimer();
        _mouseEnterDelayTimer = new DispatcherTimer
        { Interval = TimeSpan.FromMilliseconds(MouseEnterDelay) };
        _mouseEnterDelayTimer.Tick += HandleMouseEnterTimerTick;
        _mouseEnterDelayTimer.Start();
    }

    private void HandleMouseEnterTimerTick(object? sender, EventArgs e)
    {
        StopMouseEnterTimer();
        ShowPickerCore();
    }

    private void StopMouseEnterTimer()
    {
        if (_mouseEnterDelayTimer != null)
        {
            _mouseEnterDelayTimer.Stop();
            _mouseEnterDelayTimer.Tick -= HandleMouseEnterTimerTick;
            _mouseEnterDelayTimer = null;
        }
    }

    private void StartMouseLeaveTimer()
    {
        StopMouseLeaveTimer();
        _mouseLeaveDelayTimer = new DispatcherTimer
        { Interval = TimeSpan.FromMilliseconds(MouseLeaveDelay) };
        _mouseLeaveDelayTimer.Tick += HandleMouseLeaveTimerTick;
        _mouseLeaveDelayTimer.Start();
    }

    private void HandleMouseLeaveTimerTick(object? sender, EventArgs e)
    {
        StopMouseLeaveTimer();
        HidePickerCore();
    }

    private void StopMouseLeaveTimer()
    {
        if (_mouseLeaveDelayTimer != null)
        {
            _mouseLeaveDelayTimer.Stop();
            _mouseLeaveDelayTimer.Tick -= HandleMouseLeaveTimerTick;
            _mouseLeaveDelayTimer = null;
        }
    }

    #endregion

    #region Scope helpers

    private bool IsVisualInPickerScope(Visual visual)
        => PopupUtils.IsVisualInPopupScope(visual, this, _popup?.Child as Visual);

    private bool IsFocusWithinPickerScope()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        return topLevel?.FocusManager.GetFocusedElement() is Visual focused
               && IsVisualInPickerScope(focused);
    }

    #endregion

    #region ShowPicker / HidePicker

    internal void ShowPicker(bool immediately = false)
    {
        if (!IsEnabled || !IsVisible)
        {
            return;
        }

        StopMouseLeaveTimer();

        if (immediately || MouseEnterDelay == 0)
        {
            ShowPickerCore();
        }
        else
        {
            StartMouseEnterTimer();
        }
    }

    private void ShowPickerCore()
    {
        _isPickerShowing = true;
        StopMouseEnterTimer();
        StopMouseLeaveTimer();

        if (IsPickerOpen && _popup is { IsPlayingCloseMotion: true })
        {
            _popup.CancelCloseAnimation();
            _isPickerShowing = false;
            return;
        }

        SetCurrentValue(IsPickerOpenProperty, true);
    }

    internal void HidePicker(bool immediately = false)
    {
        StopMouseEnterTimer();

        if (immediately || MouseLeaveDelay == 0)
        {
            HidePickerCore();
        }
        else
        {
            StartMouseLeaveTimer();
        }
    }

    private void HidePickerCore()
    {
        StopMouseLeaveTimer();
        SetCurrentValue(IsPickerOpenProperty, false);
    }

    #endregion

    #endregion

    #region 箭头管理

    private void ConfigureShowArrowEffective()
    {
        if (!IsArrowVisible)
        {
            IsArrowVisibleEffective = false;
        }
        else
        {
            IsArrowVisibleEffective = PopupUtils.CanEnabledArrow(Placement, PlacementAnchor, PlacementGravity);
        }
    }

    private void ConfigureArrowPosition()
    {
        var arrowPosition = PopupUtils.CalculateArrowPosition(Placement, PlacementAnchor, PlacementGravity);
        if (arrowPosition.HasValue)
        {
            arrowPosition = ArrowPositionUtils.FlipArrowPosition(
                arrowPosition.Value, IsPopupHorizontalFlipped, IsPopupVerticalFlipped);
            SetCurrentValue(ArrowPositionProperty, arrowPosition);
        }
    }

    #endregion

    protected abstract void GenerateValueText();
    protected abstract void GenerateColorBlockBackground();

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsTextVisibleProperty || change.Property == FormatProperty)
        {
            GenerateValueText();
        }
        else if (change.Property == CornerRadiusProperty ||
                 change.Property == CompactSpaceItemPositionProperty ||
                 change.Property == CompactSpaceOrientationProperty)
        {
            ConfigureCornerRadius();
        }

        if (change.Property == IsArrowVisibleProperty ||
            change.Property == PlacementProperty ||
            change.Property == PlacementAnchorProperty ||
            change.Property == PlacementGravityProperty)
        {
            ConfigureShowArrowEffective();
        }

        if (change.Property == PlacementProperty ||
            change.Property == PlacementAnchorProperty ||
            change.Property == PlacementGravityProperty ||
            change.Property == IsPopupHorizontalFlippedProperty ||
            change.Property == IsPopupVerticalFlippedProperty)
        {
            ConfigureArrowPosition();
        }

        if (change.Property == PlacementProperty)
        {
            ConfigurePopupMotion();
        }

        if (change.Property == SizeTypeProperty ||
            change.Property == HeightProperty ||
            change.Property == TriggerPaddingProperty ||
            change.Property == BorderThicknessProperty)
        {
            ConfigureColorBlockSize();
        }

        if (change.Property == IsNeedConfirmProperty && IsPickerOpen && !IsConfirmationOperationActive)
        {
            ResetConfirmationDraft();
        }

        if (change.Property == IsPopupPinnedOpenProperty)
        {
            ApplyPopupTriggerSettings();

            if (change.GetNewValue<bool>())
            {
                SetCurrentValue(IsPickerOpenProperty, true);
            }
            else if (IsPickerOpen)
            {
                if (_popup is { IsOpen: true })
                {
                    SetCurrentValue(IsPickerOpenProperty, true);
                }
                else
                {
                    ClosePickerForLifecycle();
                }
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (IsNeedConfirm && PickerPresenter is AbstractColorPickerView view)
        {
            view.EndEdit();
        }
        base.OnApplyTemplate(e);
        DetachPopupHandlers();
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        if (_popup != null)
        {
            if (!IsPopupPinnedOpen)
            {
                _popup.OverlayInputPassThroughElement = this;
            }
            AttachPopupHandlers();

            // Popup's computed flip properties are read-only binding sources.
            // TemplateBinding cannot target them or express OneWayToSource.
            this[!IsPopupHorizontalFlippedProperty] = _popup[!Popup.IsHorizontalFlippedProperty];
            this[!IsPopupVerticalFlippedProperty] = _popup[!Popup.IsVerticalFlippedProperty];


            ApplyPopupTriggerSettings();
        }
        SetupPopupProperties();
        ConfigureShowArrowEffective();
        ConfigureArrowPosition();
        ConfigureColorBlockSize();
        // The business state can become open before the template has produced PART_Popup.
        // Defer the physical open until placement, pinning and dismiss behavior are ready.
        if (IsPickerOpen && _popup != null && !_popup.IsOpen)
        {
            _popup.IsOpen = true;
        }
    }

    private void ConfigureColorBlockSize()
    {
        if (SizeType != CustomizableSizeType.Custom || double.IsNaN(Height))
        {
            ColorBlockSize = double.NaN;
            return;
        }

        var colorBlockSize = Height -
                             TriggerPadding.Top -
                             TriggerPadding.Bottom -
                             BorderThickness.Top -
                             BorderThickness.Bottom;
        ColorBlockSize = Math.Max(0, colorBlockSize);
    }

    private void DetachPopupHandlers()
    {
        if (_popup != null)
        {
            _popup.Opened -= HandlePopupOpened;
            _popup.Closed -= HandlePopupClosed;
        }
    }

    private void AttachPopupHandlers()
    {
        if (_popup == null)
        {
            return;
        }

        _popup.Opened -= HandlePopupOpened;
        _popup.Closed -= HandlePopupClosed;
        _popup.Opened += HandlePopupOpened;
        _popup.Closed += HandlePopupClosed;
    }

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        if (IsNeedConfirm && PickerPresenter is AbstractColorPickerView { IsEditing: false })
        {
            ResetConfirmationDraft();
        }
        AttachConfirmationHandlers();
        if (TriggerType == FlyoutTriggerType.Hover)
        {
            SubscribeToPopupPointer();
        }
    }

    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        ReleaseConfirmationHandlers();
        if (IsNeedConfirm && PickerPresenter is AbstractColorPickerView view)
        {
            view.EndEdit();
        }
        UnsubscribeFromPopupPointer();
        // Keep the business state synchronized with light-dismiss and Escape closes.
        if (IsPickerOpen)
        {
            SetCurrentValue(IsPickerOpenProperty, false);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttachedToVisualTree = true;
        SetupTriggerHandler();
        AttachPopupHandlers();
        if (IsPickerOpen)
        {
            RegisterWindowDeactivatedHandler();
        }

        if (IsPopupPinnedOpen && !IsPickerOpen)
        {
            SetCurrentValue(IsPickerOpenProperty, true);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttachedToVisualTree = false;
        ClosePickerForLifecycle();
        base.OnDetachedFromVisualTree(e);
        UnregisterWindowDeactivatedHandler();

        StopMouseEnterTimer();
        StopMouseLeaveTimer();
        UnregisterRootPointerHandler();
        UnsubscribeFromPopupPointer();
        DetachPopupHandlers();
        _triggerSubscriptions?.Dispose();
        _triggerSubscriptions = null;
    }

    private void ClosePickerForLifecycle()
    {
        try
        {
            ++_popupLifecycleCloseDepth;
            _popup?.CloseForLifecycle();
            SetCurrentValue(IsPickerOpenProperty, false);
        }
        finally
        {
            --_popupLifecycleCloseDepth;
        }
    }

    private void HandleWindowDeactivated(object? sender, EventArgs e)
    {
        SetCurrentValue(IsPickerOpenProperty, false);
    }

    private void RegisterWindowDeactivatedHandler()
    {
        RegisterWindowDeactivatedHandler(TopLevel.GetTopLevel(this));
    }

    private void RegisterWindowDeactivatedHandler(TopLevel? topLevel)
    {
        if (ReferenceEquals(_deactivationTopLevel, topLevel))
        {
            return;
        }

        UnregisterWindowDeactivatedHandler();
        if (topLevel is null)
        {
            return;
        }

        _deactivationTopLevel = topLevel;
        _deactivationSubscription = TopLevelDeactivation.Subscribe(topLevel, HandleWindowDeactivated);
    }

    private void UnregisterWindowDeactivatedHandler()
    {
        _deactivationSubscription?.Dispose();
        _deactivationSubscription = null;
        _deactivationTopLevel = null;
    }

    protected virtual void SetupPopupProperties()
    {
        ConfigurePopupMotion();
    }

    private void ConfigurePopupMotion()
    {
        if (_popup == null)
        {
            return;
        }

        var (openMotion, closeMotion) = PopupUtils.CreateMotionForPlacement(Placement);
        _popup.OpenMotion = openMotion;
        _popup.CloseMotion = closeMotion;
    }

    public void ClosePicker()
    {
        SetCurrentValue(IsPickerOpenProperty, false);
    }

    internal void NotifyFormValueChanged()
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifyPickerOpened()
    {
        RegisterWindowDeactivatedHandler();

        if (PickerPresenter is null)
        {
            PickerPresenter = CreatePresenter();
            // The presenter is created at runtime in a Content subtree; the owner's
            // ControlTheme cannot bind through this independent template owner.
            if (PickerPresenter is AbstractColorPickerView view)
            {
                view[!AbstractColorPickerView.IsNeedConfirmProperty] = this[!IsNeedConfirmProperty];
            }
            NotifyPickerPresenterCreated(PickerPresenter);
        }

        AttachConfirmationHandlers();
        if (TriggerType == FlyoutTriggerType.Focus)
        {
            RegisterRootPointerHandler();
            Dispatcher.Post(() => _isPickerShowing = false);
        }
        else
        {
            _isPickerShowing = false;
        }
    }

    protected virtual void NotifyPickerClosed()
    {
        ReleaseConfirmationHandlers();
        UnregisterWindowDeactivatedHandler();
        UnregisterRootPointerHandler();
        UnsubscribeFromPopupPointer();
    }

    protected virtual void NotifyPickerPresenterCreated(Control pickerPresenter)
    {
    }

    protected abstract Control CreatePresenter();

    private void AttachConfirmationHandlers()
    {
        ReleaseConfirmationHandlers();
        if (PickerPresenter is AbstractColorPickerView view)
        {
            view.ConfirmRequested += HandleConfirmationRequested;
            view.CancelRequested += HandleCancellationRequested;
        }
    }

    private void ReleaseConfirmationHandlers()
    {
        if (PickerPresenter is AbstractColorPickerView view)
        {
            view.ConfirmRequested -= HandleConfirmationRequested;
            view.CancelRequested -= HandleCancellationRequested;
        }
    }

    private enum ConfirmationOperation
    {
        None,
        Confirming,
        Cancelling
    }

    private ConfirmationOperation _confirmationOperation;
    internal bool IsConfirmationOperationActive => _confirmationOperation != ConfirmationOperation.None;

    internal virtual void ResetConfirmationDraft()
    {
    }

    internal virtual void CommitConfirmationDraft()
    {
    }

    private void HandleConfirmationRequested(object? sender, EventArgs args)
    {
        if (!IsNeedConfirm || !IsPickerOpen || IsConfirmationOperationActive)
        {
            return;
        }
        _confirmationOperation = ConfirmationOperation.Confirming;
        try
        {
            CommitConfirmationDraft();
            CompleteConfirmationOperation();
        }
        finally
        {
            _confirmationOperation = ConfirmationOperation.None;
        }
    }

    private void HandleCancellationRequested(object? sender, EventArgs args)
    {
        if (!IsNeedConfirm || !IsPickerOpen || IsConfirmationOperationActive)
        {
            return;
        }
        _confirmationOperation = ConfirmationOperation.Cancelling;
        try
        {
            CompleteConfirmationOperation();
        }
        finally
        {
            _confirmationOperation = ConfirmationOperation.None;
        }
    }

    private void CompleteConfirmationOperation()
    {
        if (IsPopupPinnedOpen)
        {
            ResetConfirmationDraft();
        }
        else
        {
            ClosePicker();
        }
    }

    private void HandleIsPickerOpenChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is true)
        {
            NotifyPickerOpened();
            if (_popup != null && !_popup.IsOpen)
            {
                _popup.IsOpen = true;
            }
        }
        else
        {
            if (_popup is { IsOpen: true })
            {
                _popup.IsOpen = false;
            }
            NotifyPickerClosed();
        }

        UpdatePseudoClasses();
    }

    private static bool CoerceIsPickerOpen(AvaloniaObject sender, bool value)
    {
        return value ||
               sender is AbstractColorPicker { IsPopupPinnedOpen: true, _popupLifecycleCloseDepth: 0 };
    }

    protected virtual void UpdatePseudoClasses()
    {
        PseudoClasses.Set(ColorPickerPseudoClass.FlyoutOpen, IsPickerOpen);
    }

    public static string FormatColor(Color color, ColorFormat format)
    {
        if (format == ColorFormat.Hex)
        {
            return ColorToHexConverter.ToHexString(color, AlphaComponentPosition.Leading, false, true);
        }
        if (format == ColorFormat.Rgba)
        {
            return $"rgba({(int)color.R}, {(int)color.G}, {(int)color.B}, {color.GetAlphaF():0.00})";
        }

        var hsvColor = color.ToHsv();
        return $"hsva({hsvColor.H:0}, {hsvColor.S * 100:0}%, {hsvColor.V * 100:0}%,  {hsvColor.A:0.00})";
    }

    private void ConfigureCornerRadius()
    {
        EffectiveCornerRadius = CompactSpace.CalculateEffectiveCornerRadius(
            CornerRadius,
            IsUsedInCompactSpace,
            CompactSpaceItemPosition,
            CompactSpaceOrientation);
    }

    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        IsUsedInCompactSpace = position != null;
        CompactSpaceItemPosition = position;
    }

    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        CompactSpaceOrientation = orientation;
    }

    double ICompactSpaceAware.GetBorderThickness()
    {
        return CompactSpaceOrientation == Orientation.Horizontal ? BorderThickness.Left : BorderThickness.Top;
    }

    #region 实现 FormItem 接口

    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);

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
        if (status == FormValidateStatus.Error)
        {
            SetCurrentValue(StatusProperty, InputControlStatus.Error);
        }
        else if (status == FormValidateStatus.Warning)
        {
            SetCurrentValue(StatusProperty, InputControlStatus.Warning);
        }
        else
        {
            SetCurrentValue(StatusProperty, InputControlStatus.Default);
        }
    }
    #endregion

}
